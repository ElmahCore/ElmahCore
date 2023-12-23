import Vue from "vue";
import router from "./router";
import App from "./App.vue";
import FlagIcon from "vue-flag-icon";
import AsyncComputed from "vue-async-computed";
import { BootstrapVue, IconsPlugin } from "bootstrap-vue";
import VueHighlightJS from "vue-highlight.js";
import "vue-highlight.js/lib/allLanguages";
import "highlight.js/styles/default.css";
import { library } from "@fortawesome/fontawesome-svg-core";
import {
  faCopy,
  faExternalLinkAlt,
  faInfoCircle,
  faSearch,
  faFilter,
  faAngleDoubleDown,
  faCalendarAlt,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import store from "./store";
import axios from "axios";

library.add(
  faCopy,
  faExternalLinkAlt,
  faInfoCircle,
  faSearch,
  faFilter,
  faAngleDoubleDown,
  faCalendarAlt
);

Vue.component("font-awesome-icon", FontAwesomeIcon);

Vue.config.productionTip = false;

Vue.use(require("vue-moment"));
Vue.use(FlagIcon);
Vue.use(AsyncComputed);
Vue.use(BootstrapVue);
Vue.use(IconsPlugin);
Vue.use(VueHighlightJS);

axios.defaults.headers["x-requested-with"] = "XMLHttpRequest";

new Vue({
  router,
  render: (h) => h(App),
  store: store,
}).$mount("#app");
