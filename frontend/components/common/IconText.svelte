<script lang="ts">
    import { afterUpdate } from 'svelte'

    import { iconStrings, imageStrings } from '@/data/icons'

    import IconifyIcon from '@/components/images/IconifyIcon.svelte'
    import WowthingImage from '@/components/images/sources/WowthingImage.svelte'

    export let dropShadow = false
    export let text: string

    let element: HTMLElement
    let html: string
    $: {
        html = text.replaceAll(/:(.*?):/g, '<span data-string="$1"></span>')
    }

    afterUpdate(() => {
        const spans = element.querySelectorAll('[data-string]')
        for (const span of spans) {
            const dataString = span.getAttribute('data-string')

            if (iconStrings[dataString] || !imageStrings[dataString]) {
                new IconifyIcon({
                    target: span,
                    props: {
                        icon: iconStrings[dataString] || iconStrings.question,
                        scale: '0.9',
                        dropShadow,
                    }
                })
            }
            else {
                new WowthingImage({
                    target: span,
                    props: {
                        border: 0,
                        name: imageStrings[dataString],
                        size: 20,
                    }
                })
            }
        }
    })
</script>

<style lang="scss">
    span {
        :global(svg) {
            margin-top: -4px;
        }
    }
</style>

<span
    bind:this={element}
    class="text-overflow"
>
    {@html html}
</span>
