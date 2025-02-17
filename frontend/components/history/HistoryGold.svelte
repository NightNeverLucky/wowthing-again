<script lang="ts">
    import {
        Chart,
        LineElement,
        PointElement,
        LineController,
        LinearScale,
        LogarithmicScale,
        TimeScale,
        Filler,
        Legend,
        Title,
        Tooltip,
    } from 'chart.js'
    import type { TimeUnit } from 'chart.js'
    import 'chartjs-adapter-luxon'
    import some from 'lodash/some'
    import sortBy from 'lodash/sortBy'
    import toPairs from 'lodash/toPairs'
    import { onMount } from 'svelte'
    import type { DateTime } from 'luxon'

    import { colors } from '@/data/colors'
    import { staticStore, timeStore, userHistoryStore } from '@/stores'
    import { historyState } from '@/stores/local-storage'
    import { Region } from '@/types/enums'
    import parseApiTime from '@/utils/parse-api-time'
    import type { HistoryState } from '@/stores/local-storage'
    import type { StaticDataRealm } from '@/types/data/static'

    import RadioGroup from '@/components/forms/RadioGroup.svelte'

    Chart.register(
        LineElement,
        PointElement,
        LineController,
        LinearScale,
        LogarithmicScale,
        TimeScale,
        Filler,
        Legend,
        Title,
        Tooltip,
    )

    let chart: Chart
    $: {
        if (document.getElementById('gold-chart')) {
            redrawChart($historyState)
        }
    }

    onMount(() => redrawChart($historyState))

    const intervalSettings: Record<string, [string, TimeUnit]> = {
        hour: ['ff', 'day'],
        day: ['DD', 'day'],
        week: ['DD', 'week'],
        month: ['MMM yyyy', 'month'],
    }

    type DataPoint = {x: DateTime, y: number}
    const redrawChart = function(historyState: HistoryState)
    {
        if (chart) {
            chart.destroy()
        }

        const data: { datasets: any[] } = {
            datasets: [],
        }
        const stacked = historyState.chartType === 'area-stacked'

        const realms: [string, number][] = []
        for (const realmId in $userHistoryStore.data.gold) {
            if (!some($userHistoryStore.data.gold[realmId], ([, value]) => value > 0)) {
                continue
            }

            const realm: StaticDataRealm = $staticStore.data.realms[realmId]
            realms.push([
                `[${Region[realm.region]}] ${realm.name}`,
                parseInt(realmId)
            ])
        }
        //realms.sort()

        const pointMap: Record<number, DateTime> = {}

        let firstRealmId = -1
        for (let realmIndex = 0; realmIndex < realms.length; realmIndex++) {
            const [realmName, realmId] = realms[realmIndex]
            if (firstRealmId === -1) {
                firstRealmId = realmId
            }

            const color = colors[(realmIndex + 1) * 2]

            let points: DataPoint[]
            if (historyState.interval === 'hour') {
                points = $userHistoryStore.data.gold[realmId].map((point) => ({
                    x: parseApiTime(point[0]),
                    y: point[1],
                }))
            }
            else {
                const temp: Record<string, [DateTime, number]> = {}
                for (const [time, value] of $userHistoryStore.data.gold[realmId]) {
                    const parsedTime = parseApiTime(time).toLocal()
                    let fakeTime: DateTime
                    if (historyState.interval === 'day') {
                         fakeTime = parsedTime.set({
                             hour: 0,
                             minute: 0,
                             second: 0,
                         })
                    }
                    else if (historyState.interval === 'week') {
                        fakeTime = parsedTime.set({
                            weekday: 8,
                            hour: 0,
                            minute: 0,
                            second: 0,
                        })
                    }
                    else if (historyState.interval === 'month') {
                        fakeTime = parsedTime.set({
                            day: 1,
                            hour: 0,
                            minute: 0,
                            second: 0,
                        })
                    }

                    if (fakeTime > $timeStore) {
                        fakeTime = $timeStore.set({
                            hour: 0,
                            minute: 0,
                            second: 0,
                        })
                    }

                    temp[fakeTime.toISODate()] = [fakeTime, value]
                }
                points = sortBy(
                    toPairs(temp),
                    ([date]) => date
                ).map(([, [time, value]]) => ({
                        x: time,
                        y: value,
                    })
                )
            }

            for (const point of points) {
                pointMap[point.x.toUnixInteger()] = point.x
            }

            data.datasets.push({
                backgroundColor: color,
                borderColor: stacked ? '#000' : color,
                borderWidth: stacked ? 1 : 2,
                fill: stacked,
                label: realmName,
                spanGaps: true,
                data: points,
            })
        }

        // Pad out each dataset with data for all time periods
        const totals: Record<number, DataPoint> = {}

        const allPoints: [number, DateTime][] = Object.entries(pointMap)
            .map(([a, b]) => [parseInt(a), b])
        allPoints.sort()

        for (const dataset of data.datasets) {
            const oldMap: Record<number, DataPoint> = Object.fromEntries(
                dataset.data.map((dataPoint: DataPoint) => [dataPoint.x.toUnixInteger(), dataPoint])
            )
            const newData: DataPoint[] = []

            for (let pointIndex = 0; pointIndex < allPoints.length; pointIndex++) {
                const [ts, dateTime] = allPoints[pointIndex]
                if (oldMap[ts]) {
                    newData.push(oldMap[ts])
                }
                else {
                    if (pointIndex === 0) {
                        newData.push({x: dateTime, y: 0})
                    }
                    else {
                        newData.push({x: dateTime, y: newData[pointIndex - 1].y})
                    }
                }

                if (totals[ts] === undefined) {
                    totals[ts] = {x: dateTime, y: 0}
                }
                totals[ts].y += newData[pointIndex].y
            }

            dataset.data = newData
        }

        if (historyState.scaleType === 'logarithmic') {
            data.datasets.sort((a, b) => a.data[a.data.length - 1].y - b.data[b.data.length - 1].y)
        }
        else {
            data.datasets.sort((a, b) => b.data[b.data.length - 1].y - a.data[a.data.length - 1].y)
        }

        if (historyState.chartType === 'line' && firstRealmId >= 0) {
            const totalPoints = sortBy(
                Object.values(totals),
                (point: DataPoint) => point.x.toUnixInteger()
            )

            data.datasets.push({
                backgroundColor: colors[0],
                borderColor: colors[0],
                borderWidth: 2,
                fill: stacked,
                label: 'Total',
                spanGaps: true,
                data: totalPoints,
            })
        }

        const ctx = document.getElementById('gold-chart') as HTMLCanvasElement
        chart = new Chart(ctx, {
            type: 'line',
            data,
            options: {
                animation: false,
                color: '#fff',
                responsive: true,
                spanGaps: true,
                interaction: {
                    axis: 'xy',
                    intersect: false,
                    mode: 'index',
                },
                layout: {
                    padding: 10,
                },
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            font: {
                                size: 14,
                            }
                        },
                    },
                    tooltip: {
                        bodySpacing: 5,
                        boxPadding: 3,
                        position: 'average',
                        callbacks: {
                            footer: (tooltipItems) =>
                            {
                                if (historyState.chartType === 'line') {
                                    return undefined
                                }

                                const total: number = tooltipItems.reduce((a: number, b) => a + b.parsed.y, 0)
                                return `Total: ${total.toLocaleString()}`
                            },
                        },
                        bodyFont: {
                            size: 15,
                        },
                        footerFont: {
                            size: 16,
                        },
                        titleFont: {
                            size: 16,
                        },
                    },
                },
                scales: {
                    x: {
                        type: 'time',
                        grid: {
                            color: '#666',
                        },
                        ticks: {
                            color: '#eee',
                            font: {
                                size: 14,
                            },
                        },
                        time: {
                            tooltipFormat: intervalSettings[historyState.interval][0],
                            unit: intervalSettings[historyState.interval][1],
                            displayFormats: {
                                week: 'yyyy-MM-dd',
                            }
                        },
                    },
                    y: {
                        stacked: stacked,
                        type: historyState.scaleType,
                        grid: {
                            color: '#666',
                        },
                        ticks: {
                            color: '#eee',
                            font: {
                                size: 14,
                            },
                        },
                    },
                },
            },
        })
    }
</script>

<style lang="scss">
    .radio-container {
        background: $highlight-background;
        margin-right: 0.5rem;
        padding: 0.25em 0.375em;
    }
    .thing-container {
        border: 1px solid $border-color;
        border-radius: $border-radius;
        padding: 1rem;
        width: 100%;
    }
    canvas {
        width: 100%;
    }
</style>

<div class="view">
    <div class="options-container">
        <div class="radio-container border">
            <RadioGroup
                bind:value={$historyState.chartType}
                name="chart_type"
                options={[
                    ['area-stacked', 'Area (stacked)'],
                    ['line', 'Line'],
                ]}
            />
        </div>

        <div class="radio-container border">
            <RadioGroup
                bind:value={$historyState.interval}
                name="interval"
                options={[
                    ['hour', 'Hourly'],
                    ['day', 'Daily'],
                    ['week', 'Weekly'],
                    ['month', 'Monthly'],
                ]}
            />
        </div>

        <div class="radio-container border">
            <RadioGroup
                    bind:value={$historyState.scaleType}
                    name="scale_type"
                    options={[
                    ['linear', 'Linear'],
                    ['logarithmic', 'Logarithmic'],
                ]}
            />
        </div>
    </div>

    <div class="thing-container">
        <canvas id="gold-chart"></canvas>
    </div>
</div>
