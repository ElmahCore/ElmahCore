<template>
  <div id="app">
    <div>
      <b-navbar toggleable="lg" type="dark" variant="dark">
        <b-navbar-brand href="#">ElmahCore</b-navbar-brand>

        <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>

        <b-collapse id="nav-collapse" is-nav>
          <b-navbar-nav>
            <b-nav-item :to="{ name: 'Errors' }">Errors</b-nav-item>
            <b-nav-item target="_blank" :href="elmah_root + '/rss'"
              >RSS Feeds</b-nav-item
            >
            <b-nav-item target="_blank" :href="elmah_root + '/digestrss'"
              >RSS Digest</b-nav-item
            >
            <b-nav-item target="_blank" :href="elmah_root + '/download'"
              >Download Log</b-nav-item
            >
            <b-nav-item
              target="_blank"
              :href="'https://github.com/ElmahCore/www'"
              >Help</b-nav-item
            >
            <b-nav-item :to="{ name: 'About' }">About</b-nav-item>
          </b-navbar-nav>
        </b-collapse>

        <div v-if="$route.name === 'Errors'">
          <b-input-group>
            <b-button class="mr-sm-2" variant="light" v-b-modal.filter-modal>
              <font-awesome-icon icon="filter" class="mr-sm-2" />
              <span style="font-size: 0.9rem">Add Filter</span>
            </b-button>
            <b-input-group-prepend is-text>
              <font-awesome-icon icon="search" />
            </b-input-group-prepend>
            <b-form-input
              placeholder="Search"
              v-model="searchText"
              @keydown.enter.prevent="search"
            ></b-form-input>
          </b-input-group>
        </div>
      </b-navbar>
    </div>

    <ErrorListFilter></ErrorListFilter>
    <router-view></router-view>
  </div>
</template>

<script>
import ErrorListFilter from "@/components/ErrorListFilter";
import store from "./store";

export default {
  name: "App",
  components: { ErrorListFilter },
  data: function () {
    return {
      appName: "ElmahCore",
      searchText: "",
    };
  },
  methods: {
    search() {
      store.dispatch("changeSearchText", this.searchText);
    },
  },
  computed: {
    elmah_root: function () {
      return window.$elmah_root;
    },
  },
};
</script>
<style lang="sass">
@import '../node_modules/typeface-roboto/index.css'
</style>
<style lang="scss">
@import "node_modules/bootstrap/scss/bootstrap.scss";
@import "node_modules/bootstrap-vue/src/index.scss";
body {
  font-family: "Roboto", sans-serif;
}
html,
body {
  overflow-y: hidden;
}
</style>
