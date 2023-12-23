<template>
  <div>
    <div class="e-list">
      <div
        class="e-list-content"
        id="e-view"
        v-on:scroll="scroll"
        :class="{ loading: loading }"
      >
        <template v-for="(item, index) in items">
          <ErrorListItem
            :class="{ gray: index % 2 === 0 }"
            v-bind:item="item.error"
            v-bind:log="item.log"
            v-bind:id="item.id"
            v-bind:key="item.id"
          ></ErrorListItem>
        </template>
      </div>
      <div class="total-count">
        Loaded <span>{{ items.length }}</span> of
        <span>{{ totalCount }}</span> errors
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import store from "./../store";
import ErrorListItem from "@/components/ErrorListItem";

export default {
  name: "ErrorsList",
  components: { ErrorListItem },
  data: function () {
    return {
      filter: "",
      items: [],
      totalCount: 0,
      loading: false,
      errorIndex: 0,
      loaded: false,
      loadNewTimerStarted: false,
      filtersHash: "",
      loadTimerId: null,
    };
  },
  watch: {
    "$store.state.searchText"() {
      this.loadErrors();
    },
    "$store.state.filterTags"() {
      var isNewFilter = this.filtersHash != store.getters.filtersHash;
      if (isNewFilter) {
        this.filtersHash = store.getters.filtersHash;
        this.loadErrors();
      }
    },
  },
  mounted: function () {
    this.loadErrors();
  },
  computed: {
    root: function () {
      return document.getElementById("e-view");
    },
  },
  created() {
    window.addEventListener("resize", this.handleResize);
  },
  destroyed() {
    window.removeEventListener("resize", this.handleResize);
  },
  methods: {
    handleResize() {
      if (!this.root) return;
      const height = window.innerHeight - this.root.offsetTop - 30;
      this.root.style.height = height + "px";
      if (window.innerWidth > 1024) this.$parent.collapsed = false;
    },
    scroll() {
      if (!this.root) return;
      this.root.onscroll = () => {
        if (this.loading || this.loaded) return;

        let bottomOfWindow =
          this.root.scrollTop + this.root.clientHeight ===
          this.root.scrollHeight;

        if (bottomOfWindow) {
          this.loading = true;
          // eslint-disable-next-line no-undef
          axios
            .get(
              (window.$elmah_root || "/elmah") +
                "/api/errors?i=" +
                this.errorIndex +
                "&s=50"
            )
            .then((response) => {
              if (response.data && response.data.errors.length > 0)
                this.errorIndex += response.data.errors.length;
              else this.loaded = true;
              this.loading = false;
              this.items = this.items.concat(response.data.errors);
              this.totalCount = response.data.totalCount;
            })
            .catch((error) => {
              this.loading = false;
              console.log(error);
              this.$bvToast.toast("Data loading error.", {
                variant: "danger",
                solid: true,
                noCloseButton: true,
                autoHideDelay: 2000,
              });
            });
        }
      };
    },
    loadErrors() {
      if (this.loadTimerId != null) {
        clearTimeout(this.loadTimerId);
        this.loadTimerId = null;
      }

      var filterTags = store.getters.filterTags;
      var searchText = store.getters.searchText;

      axios
        .post(
          (window.$elmah_root || "/elmah") +
            "/api/errors?p=0&s=50&q=" +
            encodeURIComponent(searchText),
          filterTags
        )
        .then((response) => {
          this.items = response.data.errors;
          this.errorIndex += response.data.errors.length;
          this.totalCount = response.data.totalCount;
          this.$parent.selected = this.items.length > 0 ? this.items[0] : null;
        })
        .catch((error) => {
          console.log(error);
          this.$bvToast.toast("Data loading error.", {
            variant: "danger",
            solid: true,
            noCloseButton: true,
            autoHideDelay: 2000,
          });
        })
        .finally(() => {
          if (!this.loadNewTimerStarted) {
            this.loadNewTimerStarted = true;
            setTimeout(() => this.loadNewErrors(this), 5000);
          }
        });
      this.handleResize();
    },
    loadNewErrors(ctx) {
      var filterTags = store.getters.filterTags;
      var searchText = store.getters.searchText;
      let id = ctx.items.length > 0 ? ctx.items[0].id : "";
      // eslint-disable-next-line no-undef
      axios
        .post(
          (window.$elmah_root || "/elmah") +
            "/api/new-errors?id=" +
            id +
            "q=" +
            encodeURIComponent(searchText),
          filterTags
        )
        .then((response) => {
          if (
            response.data &&
            response.data.errors &&
            response.data.errors.length
          ) {
            ctx.items = response.data.errors.concat(ctx.items);
            this.totalCount = response.data.totalCount;
            this.errorIndex += response.data.errors.length;
            this.$bvToast.toast(
              `${response.data.errors.length} new error(s) loaded.`,
              {
                variant: "warning",
                solid: true,
                noCloseButton: true,
                autoHideDelay: 2000,
              }
            );
          }
        })
        .catch((error) => {
          console.log(error);
          this.$bvToast.toast("Data loading error.", {
            variant: "danger",
            solid: true,
            noCloseButton: true,
            autoHideDelay: 2000,
          });
        })
        .finally(
          () =>
            (this.loadTimerId = setTimeout(() => this.loadNewErrors(ctx), 5000))
        );
    },
  },
};
</script>

<style lang="scss" scoped>
@import "src/styles/variables";
.e-list {
  width: 40%;
  max-width: 500px;
  min-width: 350px;
  flex-shrink: 0;
  border-right: 1px solid $border-main-color;

  .total-count {
    margin: 4px;
    text-align: start;
    font-size: 13px;
    span {
      font-weight: 600;
    }
  }
  .e-list-header {
    padding: 8px 10px;
    border-bottom: 1px solid $border-main-color;

    .input-group {
      flex-grow: 1;
    }
  }
  .e-list-content {
    overflow-y: scroll;
    overflow-x: hidden;
  }
}
@media screen and (max-width: 1024px) {
  .e-list {
    width: 100%;
    max-width: none;
    min-width: 200px;
  }
}
</style>
