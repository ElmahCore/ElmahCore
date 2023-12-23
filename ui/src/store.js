import Vue from "vue";
import Vuex from "vuex";

Vue.use(Vuex);

function _arrayEquals(array1, array2) {
  if (!array1 || !array2) return false;
  if (array2 === array1) return true;
  if (array1.length != array2.length) return false;

  for (var i = 0, l = array1.length; i < l; i++) {
    if (array1[i] instanceof Array && array2[i] instanceof Array) {
      if (!array1[i].equals(array2[i])) return false;
    } else if (array1[i] != array2[i]) {
      return false;
    }
  }
  return true;
}

const store = new Vuex.Store({
  state: {
    searchText: "",
    filterTags: [],
  },
  mutations: {
    updateSearchText(state, payload) {
      state.searchText = payload;
    },
    updateFilterTag(state, payload) {
      if (_arrayEquals(state.filterTags, payload)) return;
      state.filterTags = payload;
    },
  },
  actions: {
    changeSearchText(context, payload) {
      context.commit("updateSearchText", payload);
    },
    changeFilterTags(context, payload) {
      context.commit("updateFilterTag", payload);
    },
  },
  getters: {
    searchText(state) {
      return state.searchText;
    },
    filterTags(state) {
      let filterTags = [];
      state.filterTags.forEach(function (t) {
        filterTags.push(t);
      });
      return filterTags;
    },
    filtersHash(state) {
      let hash = "";
      state.filterTags.forEach(function (t) {
        hash += " | " + t;
      });
      return hash;
    },
  },
});
export default store;
