<script lang="ts">
    import {difficultyMap} from '@/data/difficulty'
    import type {CharacterWeeklyProgress} from '@/types'
    import getRaidVaultItemLevel from '@/utils/get-raid-vault-item-level'

    export let progress: CharacterWeeklyProgress

    let cls: string
    let dungeonName: string
    let itemLevel: number

    $: {
        if (progress.progress >= progress.threshold) {
            cls = 'vault-reward'
            dungeonName = difficultyMap[progress.level].name
            itemLevel = getRaidVaultItemLevel(progress)
        }
        else {
            const more = progress.threshold - progress.progress
            cls = 'vault-more'
            dungeonName = `Kill ${more} more boss${more === 1 ? '' : 'es'}`
        }
    }
</script>

<tr class="{cls}">
    <td>{progress.threshold}x</td>
    <td class="dungeon-name">{dungeonName}</td>
    {#if itemLevel}
        <td class="item-level">{itemLevel}</td>
    {:else}
        <td></td>
    {/if}
</tr>
