<script lang="ts">
    import sortBy from 'lodash/sortBy'

    import getAccountCharacters from '@/utils/get-account-characters'
    import { userStore } from '@/stores'
    import {data as settingsData} from '@/stores/settings'
    import { Language, Region } from '@/types/enums'

    import Checkbox from '@/components/forms/CheckboxInput.svelte'
    import Select from '@/components/forms/Select.svelte'
    import TextInput from '@/components/forms/TextInput.svelte'

    let apiKey = ''

    async function onClick() {
        const xsrf = document.getElementById('app').getAttribute('data-xsrf')
        const response = await fetch('/api/api-key-get', {
            headers: {
                'RequestVerificationToken': xsrf,
            },
        })

        const obj = await response.json()
        apiKey = obj['key']

        setTimeout(() => {
            apiKey = ''
        }, 8000)
    }
</script>

<style lang="scss">
    table {
        width: 100%;
    }
    th {
        background: $highlight-background;
        padding: 0.2rem 0.3rem;
    }

    .account-id {
        @include cell-width(7.0rem);
    }
    .account-tag {
        @include cell-width(4.0rem);

        text-align: center;
    }
    .account-enabled {
        @include cell-width(4.0rem);

        text-align: center;
    }
    .account-characters {
        @include cell-width(100%);
    }

    .api-key {
        align-items: center;
        display: flex;
        gap: 1rem;
        width: 100%;

        :first-child {
            flex: 0 0 18rem;
        }

        button {
            background: $button-success;
            border-radius: $border-radius;
            display: block;
            font-size: 1.2rem;
            margin: 0.3rem auto 0;
        }
        span {
            border: 1px solid $border-color;
            border-radius: $border-radius;
            color: #00ccff;
            display: inline-block;
            font-size: 0.92rem;
            height: 100%;
            margin-top: 0.3rem;
            text-align: center;
        }

        p {
            flex: 1;
            margin: 0;
        }
    }
</style>

<div class="thing-container settings-container">
    <h2>Account</h2>

    <h3>Language</h3>

    <div class="setting">
        <Select
            name="general_language"
            bind:selected={$settingsData.general.language}
            options={[
                [Language.deDE, '[deDE] Deutsch'],
                [Language.enUS, '[enUS] English'],
                [Language.esES, '[esES] Español (EU)'],
                [Language.esMX, '[esMX] Español (Latino)'],
                [Language.frFR, '[frFR] Français'],
                [Language.itIT, '[itIT] Italiano'],
                [Language.ptBR, '[ptBR] Português (Brasil)'],
                [Language.ruRU, '[ruRU] Русский'],
            ]}
        />
        <p>
            <strong>NOTE:</strong> this is a very new feature and only used in some places,
            work is ongoing. You will need to reload the page after changing this!
        </p>
    </div>

    <h3 class="space-me">Account Name</h3>

    <div class="setting">
        <TextInput
            name="general_DesiredAccountName"
            maxlength={32}
            placeholder="Desired account name"
            bind:value={$settingsData.general.desiredAccountName}
        />
        <p>
            If you would like to rename your account, change this and save changes. Your
            account should get renamed at some point in the future, log out/in to check.
            Pinging Freddie on Discord is the best way to have this done quickly.
        </p>
    </div>

    <h3 class="space-me">WoW Accounts</h3>

    <p>
        Tag is some short text that will display on the far left of character tables if
        "Account tag" is in your Common columns layout.
    </p>

    <table class="table-striped">
        <thead>
            <tr>
                <th>ID</th>
                <th>Tag</th>
                <th>Enabled</th>
                <th>Characters</th>
            </tr>
        </thead>
        <tbody>
            {#each sortBy(Object.values($userStore.data.accounts), (a) => a.accountId) as account}
                <tr>
                    <td class="account-id">
                        <code>{Region[account.region]}</code>
                        {account.accountId}
                    </td>
                    <td class="account-tag">
                        <TextInput
                            name="account_tag_${account.id}"
                            maxlength={4}
                            bind:value={account.tag}
                        />
                    </td>
                    <td class="account-enabled">
                        <Checkbox
                            name="account_enabled_${account.id}"
                            bind:value={account.enabled}
                        />
                    </td>
                    <td class="account-characters">
                        {#each getAccountCharacters($userStore.data, account.id) as character, characterIndex}
                            {characterIndex > 0 ? ', ' : ''}
                            <span class="class-{character.classId}">{character.name}</span>
                        {/each}
                    </td>
                </tr>
            {/each}
        </tbody>
    </table>

    <h3 class="space-me">API Key</h3>

    <div class="api-key">
        {#if apiKey}
            <span>{apiKey}</span>
        {:else}
            <button on:click|preventDefault={onClick}>Reveal API Key</button>
        {/if}

        <p>
            Use this API key with <a href="https://github.com/ThingEngineering/wowthing-sync">WoWthing Sync</a>
            to automate the uploading of your <code>WoWthing_Collector.lua</code>
            files.
        </p>
    </div>
</div>
