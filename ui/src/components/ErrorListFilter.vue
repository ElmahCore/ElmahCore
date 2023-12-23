<template>
  <div>
    <div v-if="filterTags.length > 0">
      <b-button
        size="sm"
        variant="light"
        @click="clearFilterTags"
        style="float: right; margin: 4px"
        >Clear filter</b-button
      >
      <b-form-tags v-model="filterTags" tag-variant="info" input-class="d-none">
        <template v-slot="{ tags, tagVariant, removeTag }">
          <div class="d-inline-block">
            <b-form-tag
              v-for="tag in tags"
              @remove="removeTag(tag)"
              :key="tag"
              :title="tag"
              :variant="tagVariant"
            >
              <b-link @click="editFilterTag(tag)" style="color: white">{{
                tag
              }}</b-link>
            </b-form-tag>
          </div>
        </template>
      </b-form-tags>
    </div>

    <b-modal
      id="filter-modal"
      ref="modal"
      :title="!filterEditMode ? 'Add filter' : 'Edit filter'"
      size="lg"
      @ok="handleOkFilterForm"
      @hide="resetFilterForm"
      centered
    >
      <form ref="form" @submit.stop.prevent="handleSubmit">
        <div class="row">
          <b-form-group
            class="col-sm"
            label="Property"
            label-for="property-input"
          >
            <b-form-select
              id="property-input"
              v-model="filterProperty"
              :options="filterProperties"
            ></b-form-select>
          </b-form-group>
          <b-form-group
            class="col-sm"
            label="Condition"
            label-for="condition-input"
          >
            <b-form-select
              id="condition-input"
              v-model="filterCondition"
              :options="filterTextConditions"
            ></b-form-select>
          </b-form-group>
          <b-form-group
            class="col-sm"
            label="Value"
            label-for="text-value-input"
          >
            <b-form-input
              id="text-value-input"
              v-model="filterTextValue"
              v-if="!filterPropertyIsDate"
            ></b-form-input>
            <b-form-datepicker
              v-model="filterDateValue"
              v-if="filterPropertyIsDate"
              :date-format-options="{
                year: 'numeric',
                month: 'numeric',
                day: 'numeric',
              }"
            ></b-form-datepicker>
            <b-form-timepicker
              class="mt-sm-2"
              v-model="filterTimeValue"
              v-if="filterPropertyIsDateTime"
              show-seconds
            ></b-form-timepicker>
          </b-form-group>
        </div>
      </form>
      <template #modal-footer="{ ok, cancel }">
        <b-button variant="secondary" class="float-right" @click="cancel()">
          Close
        </b-button>
        <b-button variant="info" class="float-right" @click="ok()">
          {{ !filterEditMode ? "Add" : "Save" }}
        </b-button>
      </template>
    </b-modal>
  </div>
</template>

<script>
import store from "./../store";
import dateFormat from "dateformat";

export default {
  name: "ErrorListFilter",
  created() {
    this.$root.$refs.ErrorListFilter = this;
  },
  data: function () {
    return {
      filterTags: [],
      filterProperty: "message",
      filterProperties: [
        { value: "application", text: "Application" },
        { value: "body", text: "Body" },
        { value: "client", text: "Client IP" },
        { value: "date-time", text: "Date/Time" },
        { value: "details", text: "Details" },
        { value: "host", text: "Host" },
        { value: "message", text: "Message" },
        { value: "method", text: "Method" },
        { value: "source", text: "Source" },
        { value: "status-code", text: "Status Code" },
        { value: "type", text: "Type" },
        { value: "url", text: "Url" },
        { value: "user", text: "User" },
      ],
      filterCondition: "=",
      filterTextConditions: [
        { value: "=", text: "Equals" },
        { value: "!=", text: "Not Equals" },
        { value: "~", text: "Contains" },
        { value: "!~", text: "Does Not Contain" },
      ],
      filterTextValue: "",
      filterDateValue: null,
      filterTimeValue: null,
      filterEditMode: false,
      changedFilterTag: null,
    };
  },
  watch: {
    filterTags(filterTags) {
      store.dispatch("changeFilterTags", filterTags);
    },
    filterPropertyIsDate(isDate) {
      if (isDate && this.filterDateValue == null) {
        this.filterDateValue = dateFormat(new Date(), "yyyy-MM-dd");
      }
    },
    filterPropertyIsDateTime(isDateTime) {
      if (isDateTime && this.filterTimeValue == null) {
        this.filterTimeValue = "00:00:00";
      }
    },
  },
  methods: {
    clearFilterTags() {
      this.filterTags = [];
    },
    resetFilterForm() {
      this.filterProperty = "message";
      this.filterCondition = "=";
      this.filterTextValue = "";
      this.filterDateValue = null;
      this.filterTimeValue = null;
      this.filterEditMode = false;
      this.changedFilterTag = null;
    },
    handleOkFilterForm(bvModalEvent) {
      bvModalEvent.preventDefault();
      this.handleSubmitFilterForm();
    },
    handleSubmitFilterForm() {
      this.$nextTick(() => {
        this.$bvModal.hide("filter-modal");
      });

      let isDate = this.filterPropertyIsDate;
      let isDateTime = this.filterPropertyIsDateTime;
      var filterTag = this.toFilterTag(
        this.filterProperty,
        this.filterCondition,
        !isDate ? this.filterTextValue : null,
        isDate ? this.filterDateValue : null,
        isDateTime ? this.filterTimeValue : null
      );

      if (this.changedFilterTag != null) {
        var i = this.filterTags.indexOf(this.changedFilterTag);
        if (i != -1) this.filterTags.splice(i, 1);
      }
      this.pushFilterTag(filterTag);
    },
    pushFilterTag(filterTag) {
      if (this.filterTags.indexOf(filterTag) == -1) {
        this.filterTags.push(filterTag);
        store.dispatch("changeFilterTags", this.filterTags);
      }
    },
    addFilterTag(filterTag) {
      this.pushFilterTag(filterTag);
    },
    editFilterTag(filterTag) {
      this.openFilterTagDialog(filterTag, true);
    },
    openFilterTagDialog(filterTag, editMode) {
      var match = filterTag.match(/([^\s]*)\s+([^\s]*)\s+(.*)/);

      this.filterProperty = match[1];
      this.filterCondition = match[2];

      let value = match[3];
      let isDate = this.filterPropertyIsDate;
      let isDateTime = this.filterPropertyIsDateTime;

      this.filterTextValue = !isDate ? value : null;
      this.filterDateValue = isDate ? value : null;
      this.filterTimeValue = isDateTime ? value.substring(11, 11 + 8) : null;
      this.filterEditMode = editMode;
      this.changedFilterTag = filterTag;

      this.$refs["modal"].show();
    },
    toFilterTag(property, condition, textValue, dateValue, timeValue) {
      let toFilterTag = property + " " + condition + " ";
      if (dateValue) {
        toFilterTag += dateValue;
        if (timeValue) toFilterTag += " " + timeValue;
      } else toFilterTag += textValue;

      return toFilterTag.trim();
    },
  },
  computed: {
    elmah_root: function () {
      return window.$elmah_root;
    },
    isErrorsPage() {
      return this.$route.name === "Errors";
    },
    filterPropertyIsDate: function () {
      return this.filterProperty == "date-time";
    },
    filterPropertyIsDateTime: function () {
      return (
        this.filterProperty == "date-time" &&
        (this.filterCondition == "=" || this.filterCondition == "!=")
      );
    },
  },
};
</script>
