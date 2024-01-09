<template>
  <div
    class="e-list-item"
    :class="{ selected: $parent.$parent.selected.id === id }"
    @click="onSelect"
  >
    <div class="e-list-item-col1" :title="item.time">
      <div :class="[item.severity.toLowerCase()]">{{ item.statusCode }}</div>
      <span>{{ item.time | moment("from", "now", true) }}</span>
    </div>
    <div class="e-list-item-col2">
      <div class="type">{{ item.type }}</div>
      <div v-if="item.method || item.url" class="request">
        <span class="method">{{ item.method }}</span>
        <span class="url">{{ item.url }}</span>
      </div>
      <div class="message">{{ item.message }}</div>
    </div>
  </div>
</template>

<script>
export default {
  name: "ErrorListItem",
  props: {
    item: { type: Object, default: () => ({}) },
    log: { type: Object, default: () => ({}) },
    id: { type: String, default: () => "" },
  },
  methods: {
    onSelect: function () {
      this.$parent.$parent.selected = {
        error: this.item,
        log: this.log,
        id: this.id,
      };
      if (window.innerWidth <= 1024)
        this.$parent.$parent.collapsed = !this.$parent.$parent.collapsed;
      else this.$parent.$parent.collapsed = false;
    },
  },
};
</script>

<style lang="scss" scoped>
@import "src/styles/variables";
.e-list-item {
  cursor: default;
  display: flex;
  flex-direction: row;
  border-bottom: 1px solid $border-sc-color;
  padding: 3px 0;
  line-height: 20px;

  &:hover {
    background-color: $border-sc-color;
  }
  &.selected {
    background-color: #d1ecf1;
  }

  .e-list-item-col1 {
    width: 56px;
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: start;
    margin-top: 5px;
    flex-direction: column;
    font-size: 10px;
    line-height: 14px;
    text-align: center;
    color: #888;

    div {
      background-color: #888;
      color: #fff;
      display: block;
      width: 28px;
      height: 28px;
      min-width: 28px;
      line-height: 28px;
      font-size: 12px;
      padding: 2px -1px;
      border-radius: 20px;
      text-align: center;
      font-weight: 600;
      margin-bottom: 2px;
    }
    div.error {
      background-color: $error-color;
    }
    div.warning {
      background-color: $warning-color;
    }
    div.success {
      background-color: $success-color;
    }
  }
  .e-list-item-col2 {
    flex-grow: 1;
    padding: 2px 6px;
    color: rgb(96, 94, 92);

    .request {
      display: flex;
      align-items: center;
      font-size: 13px;

      span.method {
        font-size: 9px;
        line-height: 12px;
        padding: 2px 5px;
        border: 1px solid #aaaaaa;
        border-radius: 4px;
        color: #fff;
        background-color: #aaaaaa;
        margin-right: 5px;
      }
    }

    .type {
      color: $black;
      font-size: 14px;
      font-weight: 500;
    }
    .message {
      font-size: 13px;
    }
  }
}
</style>
