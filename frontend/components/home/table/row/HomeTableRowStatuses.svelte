<script lang="ts">
    import { getContext } from 'svelte'

    import { Constants } from '@/data/constants'
    import type { Character } from '@/types'
    //import tippy from '@/utils/tippy'

    import WowthingImage from '@/components/images/sources/WowthingImage.svelte'

    const character: Character = getContext('character')
</script>

<style lang="scss">
    td {
        border-left: 1px solid $border-color;
        white-space: nowrap;

        & :global(img) {
            border-radius: $border-radius;
            margin-top: -4px;
        }
    }
</style>

<td>
    {#if character.isResting && character.level < Constants.characterMaxLevel}
        <WowthingImage
            name={Constants.icons.resting}
            size={20}
            border={1}
            tooltip="Resting"
        />
    {/if}
    {#if character.isWarMode}
        <WowthingImage
            name={Constants.icons.warMode}
            size={20}
            border={1}
            tooltip="War Mode"
        />
    {/if}
</td>
