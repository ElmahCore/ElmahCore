<template>
  <div class="e-view">
    <div class="e-main-content">
      <ErrorsList></ErrorsList>
      <ErrorDetail
        v-if="!collapsed && selected && selected.error"
        v-bind:item="selected.error"
        v-bind:log="selected.log"
        v-bind:id="selected.id"
      ></ErrorDetail>
    </div>
  </div>
</template>

<script>
import ErrorDetail from "@/components/ErrorDetail";
import ErrorsList from "@/components/ErrorsList";
export default {
  name: "ErrorsView",
  components: { ErrorsList, ErrorDetail },
  data: function () {
    return {
      selected: {},
      collapsed: false,
    };
  },
};
</script>

<style lang="scss">
@import "src/styles/variables";
$bg-toolbar: #f3f2f1;

.e-view {
  display: flex;
  flex-direction: column;

  .e-main-content {
    flex-grow: 1;
    display: flex;
    flex-direction: row;
  }
  .e-toolbar {
    background-color: $bg-toolbar;
    padding: 6px 10px;
    border-bottom: 1px solid $border-main-color;

    button {
      background-color: $bg-toolbar;
      font-size: 14px;
      color: $black;
      border: 0;
    }
  }
}
@media screen and (max-width: 1024px) {
  .e-view {
    .e-main-content {
      display: block;

      .e-detail {
        position: absolute;
        width: 100%;
        height: 100%;
        top: 0;
      }
    }
  }
}
</style>
