<script lang="ts">
    import { zoneMapStore } from '@/stores'
    import type { SidebarItem } from '@/types'

    import Sidebar from '@/components/sub-sidebar/SubSidebar.svelte'

    let categories: SidebarItem[]
    $: {
        categories = $zoneMapStore.data.sets.map((set) => set === null ? null : ({
            children: set.slice(1),
            ...set[0],
        }))
    }

    const percentFunc = function(entry: SidebarItem, parentEntry?: SidebarItem) {
        const slug = parentEntry ? `${parentEntry.slug}--${entry.slug}` : entry.slug
        const hasData = $zoneMapStore.data.counts[slug]
        return hasData.have / hasData.total * 100
    }
</script>

<Sidebar
    baseUrl="/zone-maps"
    items={categories}
    width="16rem"
    noVisitRoot={true}
    {percentFunc}
/>
