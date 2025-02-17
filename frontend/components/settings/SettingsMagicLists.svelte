<script lang="ts">
    import ListView from 'svelte-sortable-flat-list-view'

    import { iconStrings } from '@/data/icons'
    import type { SettingsChoice } from '@/types'

    import IconifyIcon from '@/components/images/IconifyIcon.svelte'

    export let active: SettingsChoice[]
    export let inactive: SettingsChoice[]
    export let key: string
    export let onFunc: () => void
    export let title: string

    const keyType = `item/${key}`

    const keyFunc = (item: SettingsChoice) => item.key
</script>

<style lang="scss">
    .wrapper {
        display: flex;
        gap: 0.5rem;
        margin-bottom: 0.5rem;

        & :global(.column:first-child svg) {
            color: $colour-success;
        }
        & :global(.column:last-child svg) {
            color: $colour-fail;
        }
    }

    .columns {
        flex: 1;

        h3 {
            border-bottom-width: 0;
            margin-bottom: 0;
            text-align: center;
        }
    }

    .column {
        border: 1px solid $border-color;
        display: flex;
        flex: 1;
        flex-direction: column;
        max-height: 40rem;
        overflow-y: auto;

        & :global(.defaultListView) {
            background: $highlight-background;
            border-radius: $border-radius;
            flex: 1;
            width: 100%;

            & :global(.ListItemView) {
                animation: none !important;
            }

            & :global(.selected:not(.dragged)) {
                background: $active-background !important;
            }

            & :global(svg) {
                color: #8cf;
                margin-top: 0.2rem;
                position: absolute;
                right: 0.1rem;
            }
        }
    }
</style>

<div class="columns">
    <h3>{title}</h3>
    <div class="wrapper">
        <div class="column">
            <ListView
                Key={keyFunc}
                List={active}
                Operations="copy"
                PanSpeed={0}
                SelectionLimit={1}
                DataToOffer={{ [keyType]: '' }}
                TypesToAccept={{ [keyType]: 'all' }}
                sortable={true}
                withTransitions={false}
                on:inserted-items={onFunc}
                on:removed-items={onFunc}
                on:sorted-items={onFunc}
                let:Item
            >
                {Item.name}
                <IconifyIcon icon={iconStrings.yes} />
            </ListView>
        </div>

        <div class="column">
            <ListView
                Key={keyFunc}
                List={inactive}
                Operations="copy"
                PanSpeed={0}
                SelectionLimit={1}
                DataToOffer={{ [keyType]: '' }}
                TypesToAccept={{ [keyType]: 'all' }}
                sortable={true}
                withTransitions={false}
                let:Item
            >
                {Item.name}
                <IconifyIcon icon={iconStrings.no} />
            </ListView>
        </div>
    </div>
</div>
