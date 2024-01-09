import Vue from "vue";
import VueRouter from "vue-router";
import About from "@/views/About.vue";
import Detail from "@/views/Detail";
import List from "@/views/List";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    name: "Root",
    redirect: "/errors",
  },
  // {
  //     path: window.$elmah_root || '/elmah',
  //     name: 'Home',
  //     redirect: (window.$elmah_root || '/elmah') +"/errors"
  // },
  {
    path: "/errors",
    name: "Errors",
    component: List,
  },
  {
    path: "/about",
    name: "About",
    component: About,
  },
  {
    path: "/detail/:id",
    name: "Detail",
    component: Detail,
    props: true,
  },
  {
    path: "*",
    redirect: { name: "Errors" },
  },
];

const router = new VueRouter({
  mode: "history",
  base: window.$elmah_root || "/elmah",
  routes,
  linkExactActiveClass: "exact-active",
  linkActiveClass: "active",
  scrollBehavior(to, from, savedPosition) {
    return savedPosition || { x: 0, y: 0 };
  },
});

export default router;
