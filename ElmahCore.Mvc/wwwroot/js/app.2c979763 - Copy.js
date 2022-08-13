/// <reference path="pagination.js" />
var size = 10;
var currentPage = 1;
!(function (e) {
    function d(b) {
        for (var c, a, l = b[0], d = b[1], m = b[2], g = 0, i = []; g < l.length; g++) (a = l[g]), Object.prototype.hasOwnProperty.call(j, a) && j[a] && i.push(j[a][0]), (j[a] = 0);
        for (c in d) Object.prototype.hasOwnProperty.call(d, c) && (e[c] = d[c]);
        for (k && k(b); i.length;) i.shift()();
        return h.push.apply(h, m || []), f();
    }
    function f() {
        for (var e, b = 0; b < h.length; b++) {
            for (var c = h[b], f = !0, d = 1; d < c.length; d++) 0 !== j[c[d]] && (f = !1);
            f && (h.splice(b--, 1), (e = a((a.s = c[0]))));
        }
        return e;
    }
    var g = {},
        j = { app: 0 },
        h = [];
    function a(b) {
        if (g[b]) return g[b].exports;
        var c = (g[b] = { i: b, l: !1, exports: {} });
        return e[b].call(c.exports, c, c.exports, a), (c.l = !0), c.exports;
    }
    (a.m = e),
        (a.c = g),
        (a.d = function (b, c, d) {
            a.o(b, c) || Object.defineProperty(b, c, { enumerable: !0, get: d });
        }),
        (a.r = function (a) {
            "undefined" != typeof Symbol && Symbol.toStringTag && Object.defineProperty(a, Symbol.toStringTag, { value: "Module" }), Object.defineProperty(a, "__esModule", { value: !0 });
        }),
        (a.t = function (b, c) {
            if ((1 & c && (b = a(b)), 8 & c || (4 & c && "object" == typeof b && b && b.__esModule))) return b;
            var d = Object.create(null);
            if ((a.r(d), Object.defineProperty(d, "default", { enumerable: !0, value: b }), 2 & c && "string" != typeof b))
                for (var e in b)
                    a.d(
                        d,
                        e,
                        function (a) {
                            return b[a];
                        }.bind(null, e)
                    );
            return d;
        }),
        (a.n = function (c) {
            var b =
                c && c.__esModule
                    ? function () {
                        return c.default;
                    }
                    : function () {
                        return c;
                    };
            return a.d(b, "a", b), b;
        }),
        (a.o = function (a, b) {
            return Object.prototype.hasOwnProperty.call(a, b);
        }),
        (a.p = "/");
    var b = (window.webpackJsonp = window.webpackJsonp || []),
        i = b.push.bind(b);
    (b.push = d), (b = b.slice());
    for (var c = 0; c < b.length; c++) d(b[c]);
    var k = i;
    h.push([0, "chunk-vendors"]), f();
})({
    0: function (a, c, b) {
        a.exports = b("56d7");
    },
    "22a3": function (a, b, c) { },
    "25d9": function (b, c, a) {
        "use strict";
        a("bea5");
    },
    "56d7": function (G, g, a) {
        "use strict";
        a.r(g), a("e260"), a("e6cf"), a("cca6"), a("a79d");
        var b = a("2b0e"),
            e = a("8c4f"),
            c = a("2877"),
            h = Object(c.a)(
                { name: "About" },
                function () {
                    var a = this.$createElement;
                    return (this._self._c || a)("h1", [this._v("About")]);
                },
                [],
                !1,
                null,
                "d193c20a",
                null
            ).exports,
            i = a("bc3a"),
            H = a.n(i),
            j =
                (a("99af"),
                    a("c975"),
                    a("a15b"),
                    a("d81d"),
                    a("b64b"),
                    a("2ca0"),
                {
                    name: "ErrorDetail",
                    data: function () {
                        return { collapsed: !0, countryInfo: {}, htmlStack: "", selectedTab: 0, helpHtml: {}, isMobile: window.innerWidth <= 1024 };
                    },
                    props: {
                        item: {
                            type: Object,
                            default: function () {
                                return {};
                            },
                        },
                        log: {
                            type: Object,
                            default: function () {
                                return {};
                            },
                        },
                        id: {
                            type: String,
                            default: function () {
                                return "";
                            },
                        },
                    },
                    computed: {
                        listBox: function () {
                            return document.getElementById("items-container");
                        },
                        elmah_root: function () {
                            return window.$elmah_root;
                        },
                        body: function () {
                            try {
                                return JSON.stringify(JSON.parse(this.item.body), null, 4);
                            } catch (a) { }
                            return this.item.body;
                        },
                    },
                    created: function () {
                        window.addEventListener("resize", this.handleResize);
                    },
                    destroyed: function () {
                        window.removeEventListener("resize", this.handleResize);
                    },
                    updated: function () {
                        var a = document.getElementsByClassName("src-code");
                        if (a && 0 !== a.length) {
                            for (var c = 0; c < a.length; c++) {
                                var f = a[c];
                                if (f) {
                                    var d = f.children[0].children;
                                    if (d)
                                        for (var e = d.length - 1; e > 0; e--) {
                                            var b = d[e];
                                            if (b.innerText.indexOf("// Error in this line ") > -1) {
                                                (b.style.backgroundColor = "#ef4c4c"), (b.style.color = "#fff"), (b.style.fontWeight = 600);
                                                break;
                                            }
                                        }
                                }
                            }
                            this.handleResize();
                        }
                    },
                    watch: {
                        item: function () {
                            var a = this;
                            (this.helpHtml = null),
                                (this.item.type || 0 == this.item.statusCode) && "http" !== this.item.type.toLowerCase()
                                    ? H.a
                                        .get(this.elmah_root + "/exception/" + this.item.type)
                                        .then(function (b) {
                                            return (a.helpHtml = b.data);
                                        })
                                        .catch(function (a) {
                                            return console.log(a);
                                        })
                                    : H.a
                                        .get(this.elmah_root + "/status/" + this.item.statusCode)
                                        .then(function (b) {
                                            return (a.helpHtml = b.data);
                                        })
                                        .catch(function (a) {
                                            return console.log(a);
                                        }),
                                this.item.client &&
                                !this.item.client.startsWith(":") &&
                                H.a
                                    .get("http://ip-api.com/json/" + this.item.client)
                                    .then(function (b) {
                                        return (a.countryInfo = b.data);
                                    })
                                    .catch(function (a) {
                                        return console.log(a);
                                    });
                            var b = this;
                            setTimeout(function () {
                                return (b.selectedTab = 0);
                            }, 1);
                        },
                    },
                    methods: {
                        handleResize: function () {
                            this.isMobile = window.innerWidth <= 1024;
                            var a = document.getElementsByClassName("tab-content");
                            if (a) {
                                var b = window.innerHeight - a[0].offsetTop - 10;
                                window.innerWidth <= 1024 ? (a[0].style.height = "auto") : (a[0].style.height = b + "px");
                            }
                        },
                        levelString: function (a) {
                            switch (a) {
                                case 0:
                                    return "Trace";
                                case 1:
                                    return "Debug";
                                case 2:
                                    return "Info";
                                case 3:
                                    return "Warning";
                                case 4:
                                    return "Error";
                                case 5:
                                    return "Critical";
                                default:
                                    return "None";
                            }
                        },
                        copyTextToClipboard: function () {
                            var b = this.$bvToast,
                                a = "URL: ("
                                    .concat(this.item.method, ") ")
                                    .concat(this.item.url, "\nHostname: ")
                                    .concat(this.item.hostName, "\nStatus Code: ")
                                    .concat(this.item.statusCode, "\nType: ")
                                    .concat(this.item.type, "\nMessage: ")
                                    .concat(this.item.message, "\nSource: ")
                                    .concat(this.item.source, "\nUser: ")
                                    .concat(this.item.user, "\nClient IP: ")
                                    .concat(this.item.client, "\nTime: ")
                                    .concat(this.item.time, "\nDetail:\n")
                                    .concat(this.item.detail, "\n");
                            if (this.item.header && 0 !== Object.keys(this.item.header).length) {
                                a += "\nHeader:\n";
                                var c = this.item;
                                a += Object.keys(this.item.header)
                                    .map(function (a) {
                                        return "	" + a + ":" + c.header[a];
                                    })
                                    .join("\n");
                            }
                            if (this.item.cookies && 0 !== Object.keys(this.item.cookies).length) {
                                a += "\nCookies:\n";
                                var d = this.item;
                                a += Object.keys(this.item.cookies)
                                    .map(function (a) {
                                        return "	" + a + ":" + d.cookies[a];
                                    })
                                    .join("\n");
                            }
                            if (this.item.form && 0 !== Object.keys(this.item.form).length) {
                                a += "\nForm:\n";
                                var e = this.item;
                                a += Object.keys(this.item.form)
                                    .map(function (a) {
                                        return "	" + a + ":" + e.form[a];
                                    })
                                    .join("\n");
                            }
                            if (this.item.connection && 0 !== Object.keys(this.item.connection).length) {
                                a += "\nConnection:\n";
                                var f = this.item;
                                a += Object.keys(this.item.connection)
                                    .map(function (a) {
                                        return "	" + a + ":" + f.connection[a];
                                    })
                                    .join("\n");
                            }
                            if (this.item.userData && 0 !== Object.keys(this.item.userData).length) {
                                a += "\nUser Data:\n";
                                var g = this.item;
                                a += Object.keys(this.item.userData)
                                    .map(function (a) {
                                        return "	" + a + ":" + g.userData[a];
                                    })
                                    .join("\n");
                            }
                            if (this.item.session && 0 !== Object.keys(this.item.session).length) {
                                a += "\nSession:\n";
                                var h = this.item;
                                a += Object.keys(this.item.session)
                                    .map(function (a) {
                                        return "	" + a + ":" + h.session[a];
                                    })
                                    .join("\n");
                            }
                            if (this.item.serverVariables && 0 !== Object.keys(this.item.serverVariables).length) {
                                a += "\nServer Variables:\n";
                                var i = this.item;
                                a += Object.keys(this.item.serverVariables)
                                    .map(function (a) {
                                        return "	" + a + ":" + i.serverVariables[a];
                                    })
                                    .join("\n");
                            }
                            navigator.clipboard
                                ? navigator.clipboard.writeText(a).then(
                                    function () {
                                        b.toast("Error copied to clipboard.", { variant: "success", solid: !0, noCloseButton: !0, autoHideDelay: 2e3 });
                                    },
                                    function (a) {
                                        console.error("Async: Could not copy text: ", a);
                                    }
                                )
                                : (function (c) {
                                    var a = document.createElement("textarea");
                                    (a.value = c), (a.style.top = "0"), (a.style.left = "0"), (a.style.position = "fixed"), document.body.appendChild(a), a.focus(), a.select();
                                    try {
                                        document.execCommand("copy"), b.toast("Error copied to clipboard.", { variant: "success", solid: !0, noCloseButton: !0, autoHideDelay: 2e3 });
                                    } catch (d) {
                                        console.error("Fallback: Oops, unable to copy", d);
                                    }
                                    document.body.removeChild(a);
                                })(a);
                        },
                    },
                }),
            k =
                (a("cacc"),
                    Object(c.a)(
                        j,
                        function () {
                            var a = this,
                                c = a.$createElement,
                                b = a._self._c || c;
                            return b("div", { staticClass: "e-detail item-row", class: { "is-mobile": a.isMobile } }, [
                                b("div", { staticClass: "item-info" }, [
                                    b("div", { staticClass: "item-info-panel" }, [
                                        b("div", [
                                            b("div", { staticClass: "toolbar" }, [
                                                a.isMobile
                                                    ? b(
                                                        "button",
                                                        {
                                                            staticClass: "btn btn-m btn-primary",
                                                            on: {
                                                                click: function (b) {
                                                                    a.$parent.collapsed = !a.$parent.collapsed;
                                                                },
                                                            },
                                                        },
                                                        [a._v(" < Return to list ")]
                                                    )
                                                    : a._e(),
                                                b("button", { staticClass: "btn btn-m btn-outline-info", attrs: { title: "Copy error" }, on: { click: a.copyTextToClipboard } }, [b("font-awesome-icon", { attrs: { icon: "copy" } })], 1),
                                                b("a", { staticClass: "btn btn-m btn-outline-info", attrs: { type: "button", target: "_blank", href: (a.elmah_root || "/elmah") + "/xml?id=" + a.id, title: "Download as XML" } }, [
                                                    b("span", [a._v("xml")]),
                                                ]),
                                                b("a", { staticClass: "btn btn-m btn-outline-info", attrs: { type: "button", target: "_blank", href: (a.elmah_root || "/elmah") + "/json?id=" + a.id, title: "Download as JSON" } }, [
                                                    b("span", [a._v("json")]),
                                                ]),
                                                b(
                                                    "a",
                                                    { staticClass: "btn btn-m btn-outline-info", attrs: { type: "button", target: "_blank", href: (a.elmah_root || "/elmah") + "/detail/" + a.id, title: "Open in new window" } },
                                                    [b("font-awesome-icon", { attrs: { icon: "external-link-alt" } })],
                                                    1
                                                ),
                                            ]),
                                            b("div", { staticClass: "item-header" }, [
                                                b("div", { staticClass: "item-subheader" }, [
                                                    b("div", { staticClass: "status", class: [a.item.severity] }, [a._v(a._s(a.item.statusCode))]),
                                                    b("div", [b("h3", { staticClass: "message" }, [a._v(a._s(a.item.message))]), b("h6", { staticClass: "text-info" }, [a._v(a._s(a.item.type))])]),
                                                ]),
                                            ]),
                                        ]),
                                        b("div", { staticClass: "item-details" }, [
                                            b("div", { staticClass: "item-details-tab" }, [
                                                b("table", [
                                                    b("tr", [b("th", [a._v("When")]), b("td", [a._v(" " + a._s(a.item.time) + " ")])]),
                                                    b("tr", [
                                                        b("th", [a._v("URL")]),
                                                        b(
                                                            "td",
                                                            [
                                                                a.item.url
                                                                    ? [
                                                                        b("span", { staticClass: "method" }, [a._v(a._s(a.item.method))]),
                                                                        b("a", { attrs: { target: "_blank", href: a.item.url } }, [a._v(a._s(a.item.url) + " "), b("font-awesome-icon", { attrs: { icon: "external-link-alt" } })], 1),
                                                                    ]
                                                                    : a._e(),
                                                            ],
                                                            2
                                                        ),
                                                    ]),
                                                    b("tr", [
                                                        b("th", [a._v("Client IP")]),
                                                        b(
                                                            "td",
                                                            [
                                                                a.countryInfo.countryCode ? b("flag", { attrs: { iso: a.countryInfo.countryCode, title: a.countryInfo.country || a.countryInfo.countryCode } }) : a._e(),
                                                                a.item.client
                                                                    ? b(
                                                                        "a",
                                                                        { attrs: { target: "_blank", href: "https://db-ip.com/" + a.item.client } },
                                                                        [a._v(a._s(a.item.client) + " "), b("font-awesome-icon", { attrs: { icon: "external-link-alt" } })],
                                                                        1
                                                                    )
                                                                    : a._e(),
                                                            ],
                                                            1
                                                        ),
                                                    ]),
                                                ]),
                                                b("table", [
                                                    b("tr", [b("th", [a._v("Application")]), b("td", [a._v(a._s(a.item.applicationName))])]),
                                                    b("tr", [b("th", [a._v("Source")]), b("td", [a._v(a._s(a.item.source))])]),
                                                    b("tr", [b("th", [a._v("User")]), b("td", [a._v(a._s(a.item.user))])]),
                                                ]),
                                            ]),
                                            b("div", { staticClass: "spacer" }),
                                            b("div", { staticClass: "request-info" }, [
                                                b("div", { staticClass: "os" }, [
                                                    "Windows" === a.item.os
                                                        ? b(
                                                            "svg",
                                                            {
                                                                staticStyle: { "enable-background": "new 0 0 305 305" },
                                                                attrs: {
                                                                    version: "1.1",
                                                                    xmlns: "http://www.w3.org/2000/svg",
                                                                    "xmlns:xlink": "http://www.w3.org/1999/xlink",
                                                                    x: "0px",
                                                                    y: "0px",
                                                                    width: "50px",
                                                                    height: "60px",
                                                                    viewBox: "0 0 305 305",
                                                                    "xml:space": "preserve",
                                                                },
                                                            },
                                                            [
                                                                b("g", { attrs: { id: "XMLID_108_" } }, [
                                                                    b("path", {
                                                                        attrs: {
                                                                            id: "XMLID_109_",
                                                                            d:
                                                                                "M139.999,25.775v116.724c0,1.381,1.119,2.5,2.5,2.5H302.46c1.381,0,2.5-1.119,2.5-2.5V2.5\n                                    c0-0.726-0.315-1.416-0.864-1.891c-0.548-0.475-1.275-0.687-1.996-0.583L142.139,23.301\n                                    C140.91,23.48,139.999,24.534,139.999,25.775z",
                                                                        },
                                                                    }),
                                                                    b("path", {
                                                                        attrs: {
                                                                            id: "XMLID_110_",
                                                                            d:
                                                                                "M122.501,279.948c0.601,0,1.186-0.216,1.644-0.616c0.544-0.475,0.856-1.162,0.856-1.884V162.5\n                                    c0-1.381-1.119-2.5-2.5-2.5H2.592c-0.663,0-1.299,0.263-1.768,0.732c-0.469,0.469-0.732,1.105-0.732,1.768l0.006,98.515\n                                    c0,1.25,0.923,2.307,2.16,2.477l119.903,16.434C122.274,279.94,122.388,279.948,122.501,279.948z",
                                                                        },
                                                                    }),
                                                                    b("path", {
                                                                        attrs: {
                                                                            id: "XMLID_138_",
                                                                            d:
                                                                                "M2.609,144.999h119.892c1.381,0,2.5-1.119,2.5-2.5V28.681c0-0.722-0.312-1.408-0.855-1.883\n                                    c-0.543-0.475-1.261-0.693-1.981-0.594L2.164,42.5C0.923,42.669-0.001,43.728,0,44.98l0.109,97.521\n                                    C0.111,143.881,1.23,144.999,2.609,144.999z",
                                                                        },
                                                                    }),
                                                                    b("path", {
                                                                        attrs: {
                                                                            id: "XMLID_169_",
                                                                            d:
                                                                                "M302.46,305c0.599,0,1.182-0.215,1.64-0.613c0.546-0.475,0.86-1.163,0.86-1.887l0.04-140\n                                    c0-0.663-0.263-1.299-0.732-1.768c-0.469-0.469-1.105-0.732-1.768-0.732H142.499c-1.381,0-2.5,1.119-2.5,2.5v117.496\n                                    c0,1.246,0.918,2.302,2.151,2.476l159.961,22.504C302.228,304.992,302.344,305,302.46,305z",
                                                                        },
                                                                    }),
                                                                ]),
                                                            ]
                                                        )
                                                        : a._e(),
                                                    "Linux" === a.item.os
                                                        ? b(
                                                            "svg",
                                                            {
                                                                staticStyle: { "enable-background": "new 0 0 304.998 304.998" },
                                                                attrs: {
                                                                    version: "1.1",
                                                                    id: "Layer_1",
                                                                    xmlns: "http://www.w3.org/2000/svg",
                                                                    "xmlns:xlink": "http://www.w3.org/1999/xlink",
                                                                    x: "0px",
                                                                    y: "0px",
                                                                    width: "50px",
                                                                    height: "60px",
                                                                    viewBox: "0 0 304.998 304.998",
                                                                    "xml:space": "preserve",
                                                                },
                                                            },
                                                            [
                                                                b("g", { attrs: { id: "XMLID_91_" } }, [
                                                                    b("path", {
                                                                        attrs: {
                                                                            id: "XMLID_92_",
                                                                            d:
                                                                                "M274.659,244.888c-8.944-3.663-12.77-8.524-12.4-15.777c0.381-8.466-4.422-14.667-6.703-17.117\n                                    c1.378-5.264,5.405-23.474,0.004-39.291c-5.804-16.93-23.524-42.787-41.808-68.204c-7.485-10.438-7.839-21.784-8.248-34.922\n                                    c-0.392-12.531-0.834-26.735-7.822-42.525C190.084,9.859,174.838,0,155.851,0c-11.295,0-22.889,3.53-31.811,9.684\n                                    c-18.27,12.609-15.855,40.1-14.257,58.291c0.219,2.491,0.425,4.844,0.545,6.853c1.064,17.816,0.096,27.206-1.17,30.06\n                                    c-0.819,1.865-4.851,7.173-9.118,12.793c-4.413,5.812-9.416,12.4-13.517,18.539c-4.893,7.387-8.843,18.678-12.663,29.597\n                                    c-2.795,7.99-5.435,15.537-8.005,20.047c-4.871,8.676-3.659,16.766-2.647,20.505c-1.844,1.281-4.508,3.803-6.757,8.557\n                                    c-2.718,5.8-8.233,8.917-19.701,11.122c-5.27,1.078-8.904,3.294-10.804,6.586c-2.765,4.791-1.259,10.811,0.115,14.925\n                                    c2.03,6.048,0.765,9.876-1.535,16.826c-0.53,1.604-1.131,3.42-1.74,5.423c-0.959,3.161-0.613,6.035,1.026,8.542\n                                    c4.331,6.621,16.969,8.956,29.979,10.492c7.768,0.922,16.27,4.029,24.493,7.035c8.057,2.944,16.388,5.989,23.961,6.913\n                                    c1.151,0.145,2.291,0.218,3.39,0.218c11.434,0,16.6-7.587,18.238-10.704c4.107-0.838,18.272-3.522,32.871-3.882\n                                    c14.576-0.416,28.679,2.462,32.674,3.357c1.256,2.404,4.567,7.895,9.845,10.724c2.901,1.586,6.938,2.495,11.073,2.495\n                                    c0.001,0,0,0,0.001,0c4.416,0,12.817-1.044,19.466-8.039c6.632-7.028,23.202-16,35.302-22.551c2.7-1.462,5.226-2.83,7.441-4.065\n                                    c6.797-3.768,10.506-9.152,10.175-14.771C282.445,250.905,279.356,246.811,274.659,244.888z M124.189,243.535\n                                    c-0.846-5.96-8.513-11.871-17.392-18.715c-7.26-5.597-15.489-11.94-17.756-17.312c-4.685-11.082-0.992-30.568,5.447-40.602\n                                    c3.182-5.024,5.781-12.643,8.295-20.011c2.714-7.956,5.521-16.182,8.66-19.783c4.971-5.622,9.565-16.561,10.379-25.182\n                                    c4.655,4.444,11.876,10.083,18.547,10.083c1.027,0,2.024-0.134,2.977-0.403c4.564-1.318,11.277-5.197,17.769-8.947\n                                    c5.597-3.234,12.499-7.222,15.096-7.585c4.453,6.394,30.328,63.655,32.972,82.044c2.092,14.55-0.118,26.578-1.229,31.289\n                                    c-0.894-0.122-1.96-0.221-3.08-0.221c-7.207,0-9.115,3.934-9.612,6.283c-1.278,6.103-1.413,25.618-1.427,30.003\n                                    c-2.606,3.311-15.785,18.903-34.706,21.706c-7.707,1.12-14.904,1.688-21.39,1.688c-5.544,0-9.082-0.428-10.551-0.651l-9.508-10.879\n                                    C121.429,254.489,125.177,250.583,124.189,243.535z M136.254,64.149c-0.297,0.128-0.589,0.265-0.876,0.411\n                                    c-0.029-0.644-0.096-1.297-0.199-1.952c-1.038-5.975-5-10.312-9.419-10.312c-0.327,0-0.656,0.025-1.017,0.08\n                                    c-2.629,0.438-4.691,2.413-5.821,5.213c0.991-6.144,4.472-10.693,8.602-10.693c4.85,0,8.947,6.536,8.947,14.272\n                                    C136.471,62.143,136.4,63.113,136.254,64.149z M173.94,68.756c0.444-1.414,0.684-2.944,0.684-4.532\n                                    c0-7.014-4.45-12.509-10.131-12.509c-5.552,0-10.069,5.611-10.069,12.509c0,0.47,0.023,0.941,0.067,1.411\n                                    c-0.294-0.113-0.581-0.223-0.861-0.329c-0.639-1.935-0.962-3.954-0.962-6.015c0-8.387,5.36-15.211,11.95-15.211\n                                    c6.589,0,11.95,6.824,11.95,15.211C176.568,62.78,175.605,66.11,173.94,68.756z M169.081,85.08\n                                    c-0.095,0.424-0.297,0.612-2.531,1.774c-1.128,0.587-2.532,1.318-4.289,2.388l-1.174,0.711c-4.718,2.86-15.765,9.559-18.764,9.952\n                                    c-2.037,0.274-3.297-0.516-6.13-2.441c-0.639-0.435-1.319-0.897-2.044-1.362c-5.107-3.351-8.392-7.042-8.763-8.485\n                                    c1.665-1.287,5.792-4.508,7.905-6.415c4.289-3.988,8.605-6.668,10.741-6.668c0.113,0,0.215,0.008,0.321,0.028\n                                    c2.51,0.443,8.701,2.914,13.223,4.718c2.09,0.834,3.895,1.554,5.165,2.01C166.742,82.664,168.828,84.422,169.081,85.08z\n                                     M205.028,271.45c2.257-10.181,4.857-24.031,4.436-32.196c-0.097-1.855-0.261-3.874-0.42-5.826\n                                    c-0.297-3.65-0.738-9.075-0.283-10.684c0.09-0.042,0.19-0.078,0.301-0.109c0.019,4.668,1.033,13.979,8.479,17.226\n                                    c2.219,0.968,4.755,1.458,7.537,1.458c7.459,0,15.735-3.659,19.125-7.049c1.996-1.996,3.675-4.438,4.851-6.372\n                                    c0.257,0.753,0.415,1.737,0.332,3.005c-0.443,6.885,2.903,16.019,9.271,19.385l0.927,0.487c2.268,1.19,8.292,4.353,8.389,5.853\n                                    c-0.001,0.001-0.051,0.177-0.387,0.489c-1.509,1.379-6.82,4.091-11.956,6.714c-9.111,4.652-19.438,9.925-24.076,14.803\n                                    c-6.53,6.872-13.916,11.488-18.376,11.488c-0.537,0-1.026-0.068-1.461-0.206C206.873,288.406,202.886,281.417,205.028,271.45z\n                                     M39.917,245.477c-0.494-2.312-0.884-4.137-0.465-5.905c0.304-1.31,6.771-2.714,9.533-3.313c3.883-0.843,7.899-1.714,10.525-3.308\n                                    c3.551-2.151,5.474-6.118,7.17-9.618c1.228-2.531,2.496-5.148,4.005-6.007c0.085-0.05,0.215-0.108,0.463-0.108\n                                    c2.827,0,8.759,5.943,12.177,11.262c0.867,1.341,2.473,4.028,4.331,7.139c5.557,9.298,13.166,22.033,17.14,26.301\n                                    c3.581,3.837,9.378,11.214,7.952,17.541c-1.044,4.909-6.602,8.901-7.913,9.784c-0.476,0.108-1.065,0.163-1.758,0.163\n                                    c-7.606,0-22.662-6.328-30.751-9.728l-1.197-0.503c-4.517-1.894-11.891-3.087-19.022-4.241c-5.674-0.919-13.444-2.176-14.732-3.312\n                                    c-1.044-1.171,0.167-4.978,1.235-8.337c0.769-2.414,1.563-4.91,1.998-7.523C41.225,251.596,40.499,248.203,39.917,245.477z",
                                                                        },
                                                                    }),
                                                                ]),
                                                            ]
                                                        )
                                                        : a._e(),
                                                    "Macintosh" === a.item.os
                                                        ? b("svg", { attrs: { version: "1.1", viewBox: "0 0 42 42", xmlns: "http://www.w3.org/2000/svg", x: "0px", y: "0px", width: "50px", height: "60px" } }, [
                                                            b("path", {
                                                                attrs: {
                                                                    d:
                                                                        "m23.091 14.018v-0.342l-1.063 0.073c-0.301 0.019-0.527 0.083-0.679 0.191-0.152 0.109-0.228 0.26-0.228 0.453 0 0.188 0.075 0.338 0.226 0.449 0.15 0.112 0.352 0.167 0.604 0.167 0.161 0 0.312-0.025 0.451-0.074s0.261-0.118 0.363-0.206c0.102-0.087 0.182-0.191 0.239-0.312 0.058-0.121 0.087-0.254 0.087-0.399zm-2.091-13.768c-11.579 0-20.75 9.171-20.75 20.75 0 11.58 9.171 20.75 20.75 20.75s20.75-9.17 20.75-20.75c0-11.579-9.17-20.75-20.75-20.75zm4.028 12.299c0.098-0.275 0.236-0.511 0.415-0.707s0.394-0.347 0.646-0.453 0.533-0.159 0.842-0.159c0.279 0 0.531 0.042 0.755 0.125 0.225 0.083 0.417 0.195 0.578 0.336s0.289 0.305 0.383 0.493 0.15 0.387 0.169 0.596h-0.833c-0.021-0.115-0.059-0.223-0.113-0.322s-0.125-0.185-0.213-0.258c-0.089-0.073-0.193-0.13-0.312-0.171-0.12-0.042-0.254-0.062-0.405-0.062-0.177 0-0.338 0.036-0.481 0.107-0.144 0.071-0.267 0.172-0.369 0.302s-0.181 0.289-0.237 0.475c-0.057 0.187-0.085 0.394-0.085 0.622 0 0.236 0.028 0.448 0.085 0.634 0.056 0.187 0.136 0.344 0.24 0.473 0.103 0.129 0.228 0.228 0.373 0.296s0.305 0.103 0.479 0.103c0.285 0 0.517-0.067 0.697-0.201s0.296-0.33 0.35-0.588h0.834c-0.024 0.228-0.087 0.436-0.189 0.624s-0.234 0.348-0.396 0.481c-0.163 0.133-0.354 0.236-0.574 0.308s-0.462 0.109-0.725 0.109c-0.312 0-0.593-0.052-0.846-0.155-0.252-0.103-0.469-0.252-0.649-0.445s-0.319-0.428-0.417-0.705-0.147-0.588-0.147-0.935c-2e-3 -0.339 0.047-0.647 0.145-0.923zm-11.853-1.262h0.834v0.741h0.016c0.051-0.123 0.118-0.234 0.2-0.33 0.082-0.097 0.176-0.179 0.284-0.248 0.107-0.069 0.226-0.121 0.354-0.157 0.129-0.036 0.265-0.054 0.407-0.054 0.306 0 0.565 0.073 0.775 0.219 0.211 0.146 0.361 0.356 0.449 0.63h0.021c0.056-0.132 0.13-0.25 0.221-0.354s0.196-0.194 0.314-0.268 0.248-0.13 0.389-0.169 0.289-0.058 0.445-0.058c0.215 0 0.41 0.034 0.586 0.103s0.326 0.165 0.451 0.29 0.221 0.277 0.288 0.455 0.101 0.376 0.101 0.594v2.981h-0.87v-2.772c0-0.287-0.074-0.51-0.222-0.667-0.147-0.157-0.358-0.236-0.632-0.236-0.134 0-0.257 0.024-0.369 0.071-0.111 0.047-0.208 0.113-0.288 0.198-0.081 0.084-0.144 0.186-0.189 0.304-0.046 0.118-0.069 0.247-0.069 0.387v2.715h-0.858v-2.844c0-0.126-0.02-0.24-0.059-0.342s-0.094-0.189-0.167-0.262c-0.072-0.073-0.161-0.128-0.264-0.167-0.104-0.039-0.22-0.059-0.349-0.059-0.134 0-0.258 0.025-0.373 0.075-0.114 0.05-0.212 0.119-0.294 0.207-0.082 0.089-0.146 0.193-0.191 0.314-0.044 0.12-0.116 0.252-0.116 0.394v2.683h-0.825v-4.374zm1.893 20.939c-3.825 0-6.224-2.658-6.224-6.9s2.399-6.909 6.224-6.909 6.215 2.667 6.215 6.909c0 4.241-2.39 6.9-6.215 6.9zm7.082-16.575c-0.141 0.036-0.285 0.054-0.433 0.054-0.218 0-0.417-0.031-0.598-0.093-0.182-0.062-0.337-0.149-0.467-0.262s-0.232-0.249-0.304-0.409c-0.073-0.16-0.109-0.338-0.109-0.534 0-0.384 0.143-0.684 0.429-0.9s0.7-0.342 1.243-0.377l1.18-0.068v-0.338c0-0.252-0.08-0.445-0.24-0.576s-0.386-0.197-0.679-0.197c-0.118 0-0.229 0.015-0.331 0.044-0.102 0.03-0.192 0.072-0.27 0.127s-0.143 0.121-0.193 0.198c-0.051 0.076-0.086 0.162-0.105 0.256h-0.818c5e-3 -0.193 0.053-0.372 0.143-0.536s0.212-0.306 0.367-0.427 0.336-0.215 0.546-0.282 0.438-0.101 0.685-0.101c0.266 0 0.507 0.033 0.723 0.101s0.401 0.163 0.554 0.288 0.271 0.275 0.354 0.451 0.125 0.373 0.125 0.59v3.001h-0.833v-0.729h-0.021c-0.062 0.118-0.14 0.225-0.235 0.32-0.096 0.095-0.203 0.177-0.322 0.244-0.12 0.067-0.25 0.119-0.391 0.155zm5.503 16.575c-2.917 0-4.9-1.528-5.038-3.927h1.899c0.148 1.371 1.473 2.279 3.288 2.279 1.741 0 2.992-0.908 2.992-2.149 0-1.074-0.76-1.723-2.519-2.167l-1.714-0.426c-2.464-0.611-3.584-1.732-3.584-3.575 0-2.269 1.982-3.844 4.807-3.844 2.76 0 4.686 1.584 4.76 3.862h-1.88c-0.13-1.371-1.25-2.214-2.918-2.214-1.658 0-2.806 0.852-2.806 2.084 0 0.972 0.722 1.547 2.482 1.991l1.445 0.361c2.751 0.667 3.881 1.751 3.881 3.696-1e-3 2.482-1.964 4.029-5.095 4.029zm-12.585-12.106c-2.621 0-4.26 2.01-4.26 5.205 0 3.186 1.639 5.196 4.26 5.196 2.612 0 4.26-2.01 4.26-5.196 1e-3 -3.195-1.648-5.205-4.26-5.205z",
                                                                },
                                                            }),
                                                        ])
                                                        : a._e(),
                                                    "iPhone" === a.item.os
                                                        ? b(
                                                            "svg",
                                                            {
                                                                staticStyle: { "enable-background": "new 0 0 22.773 22.773" },
                                                                attrs: {
                                                                    version: "1.1",
                                                                    xmlns: "http://www.w3.org/2000/svg",
                                                                    "xmlns:xlink": "http://www.w3.org/1999/xlink",
                                                                    x: "0px",
                                                                    y: "0px",
                                                                    width: "50px",
                                                                    height: "60px",
                                                                    viewBox: "0 0 22.773 22.773",
                                                                    "xml:space": "preserve",
                                                                },
                                                            },
                                                            [
                                                                b("g", [
                                                                    b("g", [
                                                                        b("path", {
                                                                            attrs: {
                                                                                d:
                                                                                    "M15.769,0c0.053,0,0.106,0,0.162,0c0.13,1.606-0.483,2.806-1.228,3.675c-0.731,0.863-1.732,1.7-3.351,1.573\n                                        c-0.108-1.583,0.506-2.694,1.25-3.561C13.292,0.879,14.557,0.16,15.769,0z",
                                                                            },
                                                                        }),
                                                                        b("path", {
                                                                            attrs: {
                                                                                d:
                                                                                    "M20.67,16.716c0,0.016,0,0.03,0,0.045c-0.455,1.378-1.104,2.559-1.896,3.655c-0.723,0.995-1.609,2.334-3.191,2.334\n                                        c-1.367,0-2.275-0.879-3.676-0.903c-1.482-0.024-2.297,0.735-3.652,0.926c-0.155,0-0.31,0-0.462,0\n                                        c-0.995-0.144-1.798-0.932-2.383-1.642c-1.725-2.098-3.058-4.808-3.306-8.276c0-0.34,0-0.679,0-1.019\n                                        c0.105-2.482,1.311-4.5,2.914-5.478c0.846-0.52,2.009-0.963,3.304-0.765c0.555,0.086,1.122,0.276,1.619,0.464\n                                        c0.471,0.181,1.06,0.502,1.618,0.485c0.378-0.011,0.754-0.208,1.135-0.347c1.116-0.403,2.21-0.865,3.652-0.648\n                                        c1.733,0.262,2.963,1.032,3.723,2.22c-1.466,0.933-2.625,2.339-2.427,4.74C17.818,14.688,19.086,15.964,20.67,16.716z",
                                                                            },
                                                                        }),
                                                                    ]),
                                                                ]),
                                                            ]
                                                        )
                                                        : a._e(),
                                                    "Android" === a.item.os
                                                        ? b(
                                                            "svg",
                                                            {
                                                                staticStyle: { "enable-background": "new 0 0 553.048 553.048" },
                                                                attrs: {
                                                                    version: "1.1",
                                                                    xmlns: "http://www.w3.org/2000/svg",
                                                                    "xmlns:xlink": "http://www.w3.org/1999/xlink",
                                                                    x: "0px",
                                                                    y: "0px",
                                                                    width: "50px",
                                                                    height: "60px",
                                                                    viewBox: "0 0 553.048 553.048",
                                                                    "xml:space": "preserve",
                                                                },
                                                            },
                                                            [
                                                                b("g", [
                                                                    b("g", [
                                                                        b("path", {
                                                                            attrs: {
                                                                                d:
                                                                                    "M76.774,179.141c-9.529,0-17.614,3.323-24.26,9.969c-6.646,6.646-9.97,14.621-9.97,23.929v142.914\n                                        c0,9.541,3.323,17.619,9.97,24.266c6.646,6.646,14.731,9.97,24.26,9.97c9.522,0,17.558-3.323,24.101-9.97\n                                        c6.53-6.646,9.804-14.725,9.804-24.266V213.039c0-9.309-3.323-17.283-9.97-23.929C94.062,182.464,86.082,179.141,76.774,179.141z",
                                                                            },
                                                                        }),
                                                                        b("path", {
                                                                            attrs: {
                                                                                d:
                                                                                    "M351.972,50.847L375.57,7.315c1.549-2.882,0.998-5.092-1.658-6.646c-2.883-1.34-5.098-0.661-6.646,1.989l-23.928,43.88\n                                        c-21.055-9.309-43.324-13.972-66.807-13.972c-23.488,0-45.759,4.664-66.806,13.972l-23.929-43.88\n                                        c-1.555-2.65-3.77-3.323-6.646-1.989c-2.662,1.561-3.213,3.764-1.658,6.646l23.599,43.532\n                                        c-23.929,12.203-42.987,29.198-57.167,51.022c-14.18,21.836-21.273,45.698-21.273,71.628h307.426\n                                        c0-25.924-7.094-49.787-21.273-71.628C394.623,80.045,375.675,63.05,351.972,50.847z M215.539,114.165\n                                        c-2.552,2.558-5.6,3.831-9.143,3.831c-3.55,0-6.536-1.273-8.972-3.831c-2.436-2.546-3.654-5.582-3.654-9.137\n                                        c0-3.543,1.218-6.585,3.654-9.137c2.436-2.546,5.429-3.819,8.972-3.819s6.591,1.273,9.143,3.819\n                                        c2.546,2.558,3.825,5.594,3.825,9.137C219.357,108.577,218.079,111.619,215.539,114.165z M355.625,114.165\n                                        c-2.441,2.558-5.434,3.831-8.971,3.831c-3.551,0-6.598-1.273-9.145-3.831c-2.551-2.546-3.824-5.582-3.824-9.137\n                                        c0-3.543,1.273-6.585,3.824-9.137c2.547-2.546,5.594-3.819,9.145-3.819c3.543,0,6.529,1.273,8.971,3.819\n                                        c2.438,2.558,3.654,5.594,3.654,9.137C359.279,108.577,358.062,111.619,355.625,114.165z",
                                                                            },
                                                                        }),
                                                                        b("path", {
                                                                            attrs: {
                                                                                d:
                                                                                    "M123.971,406.804c0,10.202,3.543,18.838,10.63,25.925c7.093,7.087,15.729,10.63,25.924,10.63h24.596l0.337,75.454\n                                        c0,9.528,3.323,17.619,9.969,24.266s14.627,9.97,23.929,9.97c9.523,0,17.613-3.323,24.26-9.97s9.97-14.737,9.97-24.266v-75.447\n                                        h45.864v75.447c0,9.528,3.322,17.619,9.969,24.266s14.73,9.97,24.26,9.97c9.523,0,17.613-3.323,24.26-9.97\n                                        s9.969-14.737,9.969-24.266v-75.447h24.928c9.969,0,18.494-3.544,25.594-10.631c7.086-7.087,10.631-15.723,10.631-25.924V185.45\n                                        H123.971V406.804z",
                                                                            },
                                                                        }),
                                                                        b("path", {
                                                                            attrs: {
                                                                                d:
                                                                                    "M476.275,179.141c-9.309,0-17.283,3.274-23.93,9.804c-6.646,6.542-9.969,14.578-9.969,24.094v142.914\n                                        c0,9.541,3.322,17.619,9.969,24.266s14.627,9.97,23.93,9.97c9.523,0,17.613-3.323,24.26-9.97s9.969-14.725,9.969-24.266V213.039\n                                        c0-9.517-3.322-17.552-9.969-24.094C493.888,182.415,485.798,179.141,476.275,179.141z",
                                                                            },
                                                                        }),
                                                                    ]),
                                                                ]),
                                                            ]
                                                        )
                                                        : a._e(),
                                                    b("div", [a._v(a._s(a.item.os))]),
                                                ]),
                                                b("div", { staticClass: "browser" }, [
                                                    "Chrome" === a.item.browser
                                                        ? b(
                                                            "svg",
                                                            {
                                                                staticStyle: { "enable-background": "new 0 0 305 305" },
                                                                attrs: {
                                                                    version: "1.1",
                                                                    xmlns: "http://www.w3.org/2000/svg",
                                                                    "xmlns:xlink": "http://www.w3.org/1999/xlink",
                                                                    x: "0px",
                                                                    y: "0px",
                                                                    width: "50px",
                                                                    height: "60px",
                                                                    viewBox: "0 0 305 305",
                                                                    "xml:space": "preserve",
                                                                },
                                                            },
                                                            [
                                                                b("g", { attrs: { id: "XMLID_16_" } }, [
                                                                    b("path", {
                                                                        attrs: {
                                                                            id: "XMLID_17_",
                                                                            d:
                                                                                "M95.506,152.511c0,31.426,25.567,56.991,56.994,56.991c31.425,0,56.99-25.566,56.99-56.991\n                                    c0-31.426-25.565-56.993-56.99-56.993C121.073,95.518,95.506,121.085,95.506,152.511z",
                                                                        },
                                                                    }),
                                                                    b("path", {
                                                                        attrs: {
                                                                            id: "XMLID_18_",
                                                                            d:
                                                                                "M283.733,77.281c0.444-0.781,0.436-1.74-0.023-2.513c-13.275-22.358-32.167-41.086-54.633-54.159\n                                    C205.922,7.134,179.441,0.012,152.5,0.012c-46.625,0-90.077,20.924-119.215,57.407c-0.643,0.804-0.727,1.919-0.212,2.81\n                                    l42.93,74.355c0.45,0.78,1.28,1.25,2.164,1.25c0.112,0,0.226-0.008,0.339-0.023c1.006-0.137,1.829-0.869,2.083-1.852\n                                    c8.465-32.799,38.036-55.706,71.911-55.706c2.102,0,4.273,0.096,6.455,0.282c0.071,0.007,0.143,0.01,0.214,0.01H281.56\n                                    C282.459,78.545,283.289,78.063,283.733,77.281z",
                                                                        },
                                                                    }),
                                                                    b("path", {
                                                                        attrs: {
                                                                            id: "XMLID_19_",
                                                                            d:
                                                                                "M175.035,224.936c-0.621-0.803-1.663-1.148-2.646-0.876c-6.457,1.798-13.148,2.709-19.889,2.709\n                                    c-28.641,0-55.038-16.798-67.251-42.794c-0.03-0.064-0.063-0.126-0.098-0.188L23.911,77.719c-0.446-0.775-1.272-1.25-2.165-1.25\n                                    c-0.004,0-0.009,0-0.013,0c-0.898,0.005-1.725,0.49-2.165,1.272C6.767,100.456,0,126.311,0,152.511\n                                    c0,36.755,13.26,72.258,37.337,99.969c23.838,27.435,56.656,45.49,92.411,50.84c0.124,0.019,0.248,0.027,0.371,0.027\n                                    c0.883,0,1.713-0.47,2.164-1.25l42.941-74.378C175.732,226.839,175.657,225.739,175.035,224.936z",
                                                                        },
                                                                    }),
                                                                    b("path", {
                                                                        attrs: {
                                                                            id: "XMLID_20_",
                                                                            d:
                                                                                "M292.175,95.226h-85.974c-1.016,0-1.931,0.615-2.314,1.555c-0.384,0.94-0.161,2.02,0.564,2.73\n                                    c14.385,14.102,22.307,32.924,22.307,53c0,15.198-4.586,29.824-13.263,42.298c-0.04,0.058-0.077,0.117-0.112,0.178l-61.346,106.252\n                                    c-0.449,0.778-0.446,1.737,0.007,2.513c0.449,0.767,1.271,1.237,2.158,1.237c0.009,0,0.019,0,0.028,0\n                                    c40.37-0.45,78.253-16.511,106.669-45.222C289.338,231.032,305,192.941,305,152.511c0-19.217-3.532-37.956-10.498-55.698\n                                    C294.126,95.855,293.203,95.226,292.175,95.226z",
                                                                        },
                                                                    }),
                                                                ]),
                                                            ]
                                                        )
                                                        : a._e(),
                                                    b("div", [a._v(a._s(a.item.browser))]),
                                                ]),
                                            ]),
                                        ]),
                                    ]),
                                    a.helpHtml && a.helpHtml.html
                                        ? b("div", { staticClass: "col-xs-12 alert alert-success msdn" }, [
                                            b("span", [b("font-awesome-icon", { attrs: { icon: "info-circle" } })], 1),
                                            b("div", [
                                                b("div", { domProps: { innerHTML: a._s(a.helpHtml.html) } }),
                                                b("div", [
                                                    b("span", { staticClass: "see-more" }, [a._v("See more: ")]),
                                                    b("a", { attrs: { target: "_blank", href: a.helpHtml.path } }, [a._v(a._s(a.helpHtml.path.indexOf("mozilla") > -1 ? "MDN" : "MSDN"))]),
                                                    a._v(" | "),
                                                    b("a", { attrs: { target: "_blank", href: "https://stackoverflow.com/search?q=" + a.item.type + " " + a.item.message } }, [a._v("Stack Overflow")]),
                                                    a._v(" | "),
                                                    b("a", { attrs: { target: "_blank", href: "https://google.com/search?q=" + a.item.type + " " + a.item.message } }, [a._v("Google")]),
                                                ]),
                                            ]),
                                        ])
                                        : a._e(),
                                ]),
                                b(
                                    "div",
                                    { staticClass: "item-additions" },
                                    [
                                        b(
                                            "b-tabs",
                                            {
                                                model: {
                                                    value: a.selectedTab,
                                                    callback: function (b) {
                                                        a.selectedTab = b;
                                                    },
                                                    expression: "selectedTab",
                                                },
                                            },
                                            [
                                                a.item.sources && a.item.sources.length > 0
                                                    ? b(
                                                        "b-tab",
                                                        { attrs: { title: "Source" } },
                                                        [
                                                            a._l(a.item.sources, function (c, d) {
                                                                return [
                                                                    b(
                                                                        "div",
                                                                        { key: d, staticClass: "sources" },
                                                                        [
                                                                            b("div", { staticClass: "source-line" }, [
                                                                                a._v(" at "),
                                                                                b("span", { staticClass: "type" }, [a._v(a._s(c.type))]),
                                                                                a._v("."),
                                                                                b("span", { staticClass: "method" }, [a._v(a._s(c.function))]),
                                                                                a._v("() in "),
                                                                                b("span", { staticClass: "filename" }, [a._v(a._s(c.fileName))]),
                                                                                a._v("("),
                                                                                b("span", { staticClass: "line" }, [a._v(a._s(c.line))]),
                                                                                a._v(") "),
                                                                            ]),
                                                                            b("highlight-code", { staticClass: "src-code", attrs: { lang: "csharp" } }, [
                                                                                a._v(
                                                                                    " " +
                                                                                    a._s(c.preContextCode + "\n") +
                                                                                    " " +
                                                                                    a._s(c.contextCode + "// Error in this line at " + c.fileName + " (" + c.line + ") \n ") +
                                                                                    " " +
                                                                                    a._s(c.postContextCode + "\n") +
                                                                                    " "
                                                                                ),
                                                                            ]),
                                                                        ],
                                                                        1
                                                                    ),
                                                                ];
                                                            }),
                                                        ],
                                                        2
                                                    )
                                                    : a._e(),
                                                a.item.htmlMessage ? b("b-tab", { key: "stack", attrs: { title: "Stack Trace", lazy: "" } }, [b("pre", { staticClass: "item-stack", domProps: { innerHTML: a._s(a.item.htmlMessage) } })]) : a._e(),
                                                a.item.body
                                                    ? b("b-tab", { key: "body", attrs: { title: "Request Body", lazy: "" } }, [b("highlight-code", { staticClass: "src-code", attrs: { lang: "json" } }, [a._v(" " + a._s(a.body) + " ")])], 1)
                                                    : a._e(),
                                                a.item.messageLog && a.item.messageLog.length > 0
                                                    ? b(
                                                        "b-tab",
                                                        {
                                                            attrs: { lazy: "" },
                                                            scopedSlots: a._u(
                                                                [
                                                                    {
                                                                        key: "title",
                                                                        fn: function () {
                                                                            return [b("span", { staticClass: "count" }, [a._v(a._s(a.item.messageLog.length))]), a._v("Log ")];
                                                                        },
                                                                        proxy: !0,
                                                                    },
                                                                ],
                                                                null,
                                                                !1,
                                                                4101144840
                                                            ),
                                                        },
                                                        a._l(a.item.messageLog, function (c) {
                                                            return b("div", { key: c.timeStamp, staticClass: "log-entry" }, [
                                                                b("div", { staticClass: "log-row" }, [
                                                                    b("div", { staticClass: "time-stamp" }, [a._v(a._s(a._f("moment")(c.timeStamp, "HH:mm:ss.SSS")))]),
                                                                    b("div", { staticClass: "level", class: [a.levelString(c.level)] }, [a._v(a._s(a.levelString(c.level)))]),
                                                                    b("div", { staticClass: "message" }, [
                                                                        a._v(a._s(c.message) + " "),
                                                                        c.exception || c.scope
                                                                            ? b(
                                                                                "a",
                                                                                {
                                                                                    attrs: { href: "#" },
                                                                                    on: {
                                                                                        click: function (a) {
                                                                                            a.preventDefault(), (c.collapsed = !c.collapsed);
                                                                                        },
                                                                                    },
                                                                                },
                                                                                [a._v(a._s(c.collapsed ? "more..." : "hide details"))]
                                                                            )
                                                                            : a._e(),
                                                                        c.params && c.params.length > 0
                                                                            ? b(
                                                                                "a",
                                                                                {
                                                                                    attrs: { href: "#" },
                                                                                    on: {
                                                                                        click: function (a) {
                                                                                            a.preventDefault(), (c.collapsed = !c.collapsed);
                                                                                        },
                                                                                    },
                                                                                },
                                                                                [a._v(a._s(c.collapsed ? "params..." : "hide params"))]
                                                                            )
                                                                            : a._e(),
                                                                    ]),
                                                                ]),
                                                                c.params && c.params.length > 0 && !c.collapsed
                                                                    ? b(
                                                                        "div",
                                                                        { staticClass: "params" },
                                                                        [
                                                                            a._l(c.params, function (c) {
                                                                                return [b("label", { key: c.timeStamp }, [a._v(a._s(c.key) + ":")]), b("highlight-code", { key: c.timeStamp, attrs: { lang: "json" } }, [a._v(a._s(c.value))])];
                                                                            }),
                                                                        ],
                                                                        2
                                                                    )
                                                                    : a._e(),
                                                                c.exception && !c.collapsed ? b("div", { staticClass: "exception" }, [b("label", [a._v("Exception")]), b("span", [a._v(a._s(c.exception))])]) : a._e(),
                                                                c.scope && !c.collapsed ? b("div", { staticClass: "scope" }, [b("label", [a._v("Scope")]), b("span", [a._v(a._s(c.scope))])]) : a._e(),
                                                            ]);
                                                        }),
                                                        0
                                                    )
                                                    : a._e(),
                                                a.item.sqlLog && a.item.sqlLog.length > 0
                                                    ? b(
                                                        "b-tab",
                                                        {
                                                            key: "sqlLog",
                                                            attrs: { lazy: "" },
                                                            scopedSlots: a._u(
                                                                [
                                                                    {
                                                                        key: "title",
                                                                        fn: function () {
                                                                            return [b("span", { staticClass: "count" }, [a._v(a._s(a.item.sqlLog.length))]), a._v("SQL ")];
                                                                        },
                                                                        proxy: !0,
                                                                    },
                                                                ],
                                                                null,
                                                                !1,
                                                                990810215
                                                            ),
                                                        },
                                                        a._l(a.item.sqlLog, function (c) {
                                                            return b(
                                                                "div",
                                                                { key: c.timeStamp },
                                                                [
                                                                    b("div", [b("span", [a._v(a._s(a._f("moment")(c.timeStamp, "HH:mm:ss.SSS")))]), a._v(" "), b("span", [a._v("(" + a._s(c.durationMs) + " ms)")])]),
                                                                    b("highlight-code", { staticClass: "src-code", attrs: { lang: "sql" } }, [a._v(" " + a._s(c.sqlText) + " ")]),
                                                                ],
                                                                1
                                                            );
                                                        }),
                                                        0
                                                    )
                                                    : a._e(),
                                                a._l(
                                                    { "Query String": a.item.queryString, Form: a.item.form, Header: a.item.header, Cookies: a.item.cookies, Connection: a.item.connection, "Server Variables": a.item.serverVariables },
                                                    function (c, d) {
                                                        return [
                                                            c && Object.keys(c).length > 0
                                                                ? b(
                                                                    "b-tab",
                                                                    {
                                                                        key: d,
                                                                        attrs: { lazy: "" },
                                                                        scopedSlots: a._u(
                                                                            [
                                                                                {
                                                                                    key: "title",
                                                                                    fn: function () {
                                                                                        return [b("span", { staticClass: "count" }, [a._v(a._s(Object.keys(c).length))]), a._v(a._s(d) + " ")];
                                                                                    },
                                                                                    proxy: !0,
                                                                                },
                                                                            ],
                                                                            null,
                                                                            !0
                                                                        ),
                                                                    },
                                                                    [
                                                                        b(
                                                                            "table",
                                                                            a._l(c, function (d, c) {
                                                                                return b("tr", { key: c }, [b("th", [a._v(a._s(c))]), b("td", [a._v(a._s(d))])]);
                                                                            }),
                                                                            0
                                                                        ),
                                                                    ]
                                                                )
                                                                : a._e(),
                                                        ];
                                                    }
                                                ),
                                            ],
                                            2
                                        ),
                                    ],
                                    1
                                ),
                            ]);
                        },
                        [],
                        !1,
                        null,
                        null,
                        null
                    )),
            f = k.exports,
            l = Object(c.a)(
                {
                    name: "Detail",
                    props: ["id"],
                    data: function () {
                        return { item: {} };
                    },
                    components: { ErrorDetail: f },
                    mounted: function () {
                        var a = this;
                        H.a
                            .get((window.$elmah_root || "/elmah") + "/api/error?id=" + this.id)
                            .then(function (b) {
                                (a.item = b.data.error), window.history.pushState("object or string", "Title", a.item.url);
                            })
                            .catch(function (b) {
                                console.log(b), a.$bvToast.toast("Data loading error.", { variant: "danger", solid: !0, noCloseButton: !0, autoHideDelay: 2e3 });
                            });
                    },
                },
                function () {
                    var b = this.$createElement,
                        a = this._self._c || b;
                    return a("div", [a("ErrorDetail", { attrs: { item: this.item, id: this.id } })], 1);
                },
                [],
                !1,
                null,
                "2b91fdb5",
                null
            ),
            m = l.exports,
            n =
                (a("d3b7"),
                    function () {
                        var b = this.$createElement,
                            a = this._self._c || b;
                        return a("div", { staticClass: "e-list-item", class: { selected: this.$parent.$parent.selected.id === this.id }, on: { click: this.onSelect } }, [
                            a("div", { staticClass: "e-list-item-col1", attrs: { title: this.item.time } }, [
                                a("div", { class: [this.item.severity.toLowerCase()] }, [this._v(this._s(this.item.statusCode))]),
                                a("span", [this._v(this._s(this._f("moment")(this.item.time, "from", "now", !0)))]),
                            ]),
                            a("div", { staticClass: "e-list-item-col2" }, [
                                a("div", { staticClass: "type" }, [this._v(this._s(this.item.type))]),
                                this.item.method || this.item.url
                                    ? a("div", { staticClass: "request" }, [a("span", { staticClass: "method" }, [this._v(this._s(this.item.method))]), a("span", { staticClass: "url" }, [this._v(this._s(this.item.url))])])
                                    : this._e(),
                                a("div", { staticClass: "message" }, [this._v(this._s(this.item.message))]),
                            ]),
                        ]);
                    }),
            o =
                (a("5896"),
                    Object(c.a)(
                        {
                            name: "ErrorListItem",
                            props: {
                                item: {
                                    type: Object,
                                    default: function () {
                                        return {};
                                    },
                                },
                                log: {
                                    type: Object,
                                    default: function () {
                                        return {};
                                    },
                                },
                                id: {
                                    type: String,
                                    default: function () {
                                        return "";
                                    },
                                },
                            },
                            methods: {
                                onSelect: function () {
                                    (this.$parent.$parent.selected = { error: this.item, log: this.log, id: this.id }),
                                        window.innerWidth <= 1024 ? (this.$parent.$parent.collapsed = !this.$parent.$parent.collapsed) : (this.$parent.$parent.collapsed = !1);
                                },
                            },
                        },
                        n,
                        [],
                        !1,
                        null,
                        "773ef169",
                        null
                    )),
            p = o.exports,
            q =
                (a("a459"),
                    Object(c.a)(
                        {
                            name: "ErrorsList",
                            components: { ErrorListItem: p },
                            data: function () {
                                return { filter: "", items: [], totalCount: 0, loading: !1, errorIndex: 0, loaded: !1, loadNewTimerStarted: !1 };
                            },
                            mounted: function () {
                                var a = this;

                                H.a
                                    .get((window.$elmah_root || "/elmah") + "/api/errors?p=" + currentPage+"&s=" + size)
                                    .then(function (b) {


                                        (a.items = b.data.errors),
                                            (a.errorIndex += b.data.errors.length),
                                            (a.totalCount = b.data.totalCount),
                                            (a.$parent.selected = a.items[0]);
                                    })
                                    .catch(function (b) {
                                        console.log(b), a.$bvToast.toast("Data loading error.", { variant: "danger", solid: !0, noCloseButton: !0, autoHideDelay: 2e3 });
                                    })
                                    .finally(function () {
                                        a.loadNewTimerStarted ||
                                            ((a.loadNewTimerStarted = !0),
                                                setTimeout(function () {
                                                    return a.loadNewErrors(a);
                                                }, 5e3));
                                    }),
                                    this.handleResize();
                            },
                            computed: {
                                root: function () {
                                    return document.getElementById("e-view");
                                },
                            },
                            created: function () {
                                window.addEventListener("resize", this.handleResize);
                            },
                            destroyed: function () {
                                window.removeEventListener("resize", this.handleResize);
                            },
                            methods: {
                                handleResize: function () {
                                    if (this.root) {
                                        var a = window.innerHeight - this.root.offsetTop - 70;
                                        (this.root.style.height = a + "px"), window.innerWidth > 1024 && (this.$parent.collapsed = !1);
                                    }
                                },
                                getPreviousRecord: function(){
                                   
                                    var a = this;
                                    alert("prev button clicked");
                                },
                                getNextRecord: function () {
                                 
                                    var b = this.$createElement,
                                        a = this._self._c || b;

                                    var a = this;
                                    currentPage = currentPage + 1;
                                    H.a
                                        .get((window.$elmah_root || "/elmah") + "/api/errors?p=" + currentPage + "&s=" + size)
                                        .then(function (b) {
                                          
                                            (a.items = b.data.errors),
                                                (a.errorIndex = b.data.errors.length),
                                                (a.totalCount = b.data.totalCount),
                                                (a.$parent.selected = a.items[0]);
                                            const elements = document.getElementsByClassName("e-list-content");
                                            while (elements.length > 0) {
                                                elements[0].parentNode.removeChild(elements[0]);
                                            }
                                            a(
                                                "div",
                                                { staticClass: "e-list-content", class: { loading: this.loading }, attrs: { id: "e-view" }, on: { scroll: this.scroll } },
                                                [
                                                    this._l(this.items, function (b, c) {
                                                        return [a("ErrorListItem", { key: b.id, class: { gray: c % 2 == 0 }, attrs: { item: b.error, log: b.log, id: b.id } })];
                                                    }),
                                                ],
                                                2
                                            )
                                        })
                                        .catch(function (b) {
                                            console.log(b), a.$bvToast.toast("Data loading error.", { variant: "danger", solid: !0, noCloseButton: !0, autoHideDelay: 2e3 });
                                        })
                                        .finally(function () {
                                          
                                        }),
                                        this.handleResize();
                                },
                                scroll: function () {
                                    var a = this;
                                    this.root &&
                                        (this.root.onscroll = function () {
                                            a.loading ||
                                                a.loaded ||
                                                (a.root.scrollTop + a.root.clientHeight === a.root.scrollHeight &&
                                                    ((a.loading = !0),
                                                        H.a
                                                            .get((window.$elmah_root || "/elmah") + "/api/errors?i=" + a.errorIndex + "&s=100")
                                                            .then(function (b) {
                                                                b.data && b.data.errors.length > 0 ? (a.errorIndex += b.data.errors.length) : (a.loaded = !0),
                                                                    (a.loading = !1),
                                                                    (a.items = a.items.concat(b.data.errors)),
                                                                    (a.totalCount = b.data.totalCount);
                                                            })
                                                            .catch(function (b) {
                                                                (a.loading = !1), console.log(b), a.$bvToast.toast("Data loading error.", { variant: "danger", solid: !0, noCloseButton: !0, autoHideDelay: 2e3 });
                                                            })));
                                        });
                                },
                                loadNewErrors: function (a) {
                                    var c = this,
                                        b = a.items.length > 0 ? a.items[0].id : "";
                                    H.a
                                        .get((window.$elmah_root || "/elmah") + "/api/new-errors?id=" + b)
                                        .then(function (b) {
                                            b.data &&
                                                b.data.errors &&
                                                b.data.errors.length > 0 &&
                                                ((a.items = b.data.errors.concat(a.items)),
                                                    /* (c.totalCount = b.data.totalCount),*/
                                                    (c.errorIndex += b.data.errors.length),
                                                    c.$bvToast.toast("".concat(b.data.errors.length, " new error(s) loaded."), { variant: "warning", solid: !0, noCloseButton: !0, autoHideDelay: 2e3 }));
                                        })
                                        .catch(function (a) {
                                            console.log(a), c.$bvToast.toast("Data loading error.", { variant: "danger", solid: !0, noCloseButton: !0, autoHideDelay: 2e3 });
                                        })
                                        .finally(function () {
                                            return setTimeout(function () {
                                                return c.loadNewErrors(a);
                                            }, 5e3);
                                        });
                                },
                            },
                        },
                        function () {
                            var b = this.$createElement,
                                a = this._self._c || b;
                            var totalPages = Math.round((this.totalCount + size - 1) / size);
                            return this.totalCount < this.items.length
                                ? a("div", { staticClass: "e-list" }, [
                                    a(
                                        "div",
                                        { staticClass: "e-list-content", class: { loading: this.loading }, attrs: { id: "e-view" }, on: { scroll: this.scroll } },
                                        [
                                            this._l(this.items, function (b, c) {
                                                return [a("ErrorListItem", { key: b.id, class: { gray: c % 2 == 0 }, attrs: { item: b.error, log: b.log, id: b.id } })];
                                            }),
                                        ],
                                        2
                                    ),
                                    a("div", { staticClass: "total-count" }, [this._v("Loaded "), a("span", [this._v(this._s(this.totalCount))]), this._v(" of "), a("span", [this._v(this._s(this.items.length))]), this._v(" errors")]),
                                ])
                                : a("div", { staticClass: "e-list" }, [
                                    a(
                                        "div",
                                        { staticClass: "e-list-content", class: { loading: this.loading }, attrs: { id: "e-view" }, on: { scroll: this.scroll } },
                                        [
                                            this._l(this.items, function (b, c) {
                                                return [a("ErrorListItem", { key: b.id, class: { gray: c % 2 == 0 }, attrs: { item: b.error, log: b.log, id: b.id } })];
                                            }),
                                        ],
                                        2
                                    ),
                                    
                                    a("div", { staticClass: "total-count" }, [this._v("Loaded "), a("span", [this._v(this._s(this.items.length))]), this._v(" of "), a("span", [this._v(this._s(this.totalCount))]), this._v(" errors")]),
                                    a(
                                        "div",
                                        { staticClass: "", class: {  }, attrs: { id: "" }, on: {  } },
                                        [
                                            a("a", { staticClass: "btn btn-outline-info", attrs: { type: "button", title: "Previous Page" },on: { click: this.getPreviousRecord } }, [
                                                a("span", [this._v("Previous")]),
                                            ]),
                                            a("span", { staticClass: "total-count" }, [this._v("Page " + currentPage + " of "), this._v(totalPages)]),

                                            a("a", { staticClass: "btn btn-outline-info", attrs: { type: "button", title: "Next Page" }, on: { click: this.getNextRecord } }, [
                                                a("span", [this._v("Next")]),
                                            ]),
                                        ],
                                        2
                                    ),
                                ]);
                        },
                        [],
                        !1,
                        null,
                        "51ea4fcf",
                        null
                    )),
            r = q.exports,
            s =
                (a("25d9"),
                    Object(c.a)(
                        {
                            name: "ErrorsView",
                            components: { ErrorsList: r, ErrorDetail: f },
                            data: function () {
                                return { selected: {}, collapsed: !1 };
                            },
                        },
                        function () {
                            var b = this.$createElement,
                                a = this._self._c || b;
                            return a("div", { staticClass: "e-view" }, [
                                a(
                                    "div",
                                    { staticClass: "e-main-content" },
                                    [a("ErrorsList"), !this.collapsed && this.selected.error ? a("ErrorDetail", { attrs: { item: this.selected.error, log: this.selected.log, id: this.selected.id } }) : this._e()],
                                    1
                                ),
                            ]);
                        },
                        [],
                        !1,
                        null,
                        null,
                        null
                    )),
            t = s.exports,
            u = Object(c.a)(
                { name: "List", components: { ErrorsView: t } },
                function () {
                    var a = this.$createElement;
                    return (this._self._c || a)("ErrorsView");
                },
                [],
                !1,
                null,
                "7cf6125a",
                null
            ),
            v = u.exports;
        b.default.use(e.a);
        var w = [
            { path: "/", name: "Root", redirect: "/errors" },
            { path: window.$elmah_root || "/elmah", name: "Home", redirect: (window.$elmah_root || "/elmah") + "/errors" },
            { path: (window.$elmah_root || "/elmah") + "/errors", name: "Errors", component: v },
            { path: (window.$elmah_root || "/elmah") + "/about", name: "About", component: h },
            { path: (window.$elmah_root || "/elmah") + "/detail/:id", name: "Detail", component: m, props: !0 },
            { path: (window.$elmah_root || "/elmah") + "*", redirect: { name: "Home" } },
        ],
            x = new e.a({
                mode: "history",
                base: "/",
                routes: w,
                linkExactActiveClass: "exact-active",
                linkActiveClass: "active",
                scrollBehavior: function (b, c, a) {
                    return a || { x: 0, y: 0 };
                },
            }),
            y =
                (a("cf25"),
                    a("5c64"),
                    Object(c.a)(
                        {
                            name: "App",
                            components: {},
                            data: function () {
                                return { appName: "ElmahCore" };
                            },
                            computed: {
                                elmah_root: function () {
                                    return window.$elmah_root;
                                },
                            },
                        },
                        function () {
                            var b = this.$createElement,
                                a = this._self._c || b;
                            return a(
                                "div",
                                { attrs: { id: "app" } },
                                [
                                    a(
                                        "div",
                                        [
                                            a(
                                                "b-navbar",
                                                { attrs: { toggleable: "lg", type: "dark", variant: "dark" } },
                                                [
                                                    a("b-navbar-brand", { attrs: { href: "#" } }, [this._v("ElmahCore")]),
                                                    a("b-navbar-toggle", { attrs: { target: "nav-collapse" } }),
                                                    a(
                                                        "b-collapse",
                                                        { attrs: { id: "nav-collapse", "is-nav": "" } },
                                                        [
                                                            a(
                                                                "b-navbar-nav",
                                                                [
                                                                    a("b-nav-item", { attrs: { to: (this.elmah_root || "/elmah") + "/errors" } }, [this._v("Errors")]),
                                                                    a("b-nav-item", { attrs: { target: "_blank", href: (this.elmah_root || "/elmah") + "/rss" } }, [this._v("RSS Feeds")]),
                                                                    a("b-nav-item", { attrs: { target: "_blank", href: (this.elmah_root || "/elmah") + "/digestrss" } }, [this._v("RSS Digest")]),
                                                                    a("b-nav-item", { attrs: { target: "_blank", href: (this.elmah_root || "/elmah") + "/download" } }, [this._v("Download Log")]),
                                                                    a("b-nav-item", { attrs: { target: "_blank", href: "https://github.com/ElmahCore/www" } }, [this._v("Help")]),
                                                                    a("b-nav-item", { attrs: { to: (this.elmah_root || "/elmah") + "/about" } }, [this._v("About")]),
                                                                ],
                                                                1
                                                            ),
                                                        ],
                                                        1
                                                    ),
                                                ],
                                                1
                                            ),
                                        ],
                                        1
                                    ),
                                    a("router-view"),
                                ],
                                1
                            );
                        },
                        [],
                        !1,
                        null,
                        null,
                        null
                    )),
            I = y.exports,
            z = a("d61f"),
            A = a("3003"),
            B = a("5f5b"),
            C = a("b1e0"),
            D = a("c964"),
            E = (a("7abb"), a("8da8"), a("ecee")),
            d = a("c074"),
            F = a("ad3d");
        E.c.add(d.a, d.b, d.c),
            b.default.component("font-awesome-icon", F.a),
            (b.default.config.productionTip = !1),
            b.default.use(a("2ead")),
            b.default.use(z.a),
            b.default.use(A.a),
            b.default.use(B.a),
            b.default.use(C.a),
            b.default.use(D.a),
            new b.default({
                router: x,
                render: function (a) {
                    return a(I);
                },
            }).$mount("#app");
    },
    5896: function (b, c, a) {
       
        "use strict";
        a("92cc");
    },
    "5c64": function (b, c, a) {
        "use strict";
        a("d32a");
    },
    "92cc": function (a, b, c) { },
    a459: function (b, c, a) {
        "use strict";
        a("b348");
    },
    b348: function (a, b, c) { },
    bea5: function (a, b, c) { },
    cacc: function (b, c, a) {
        "use strict";
        a("22a3");
    },
    cf25: function (b, c, a) {
        "use strict";
        a("fea6");
    },
    d32a: function (a, b, c) { },
    fea6: function (a, b, c) { },
});

//# sourceMappingURL=app.2c979763.js.map
