<template>
  <div class="e-detail item-row" :class="{ 'is-mobile': isMobile }">
    <div class="item-info">
      <div class="item-info-panel">
        <div>
          <div class="toolbar">
            <button
              class="btn btn-m btn-primary"
              v-if="isMobile"
              v-on:click="$parent.collapsed = !$parent.collapsed"
            >
              &lt; Return to list
            </button>
            <button
              class="btn btn-m btn-outline-info"
              v-on:click="copyTextToClipboard"
              title="Copy error"
            >
              <font-awesome-icon icon="copy" />
            </button>
            <a
              type="button"
              class="btn btn-m btn-outline-info"
              target="_blank"
              :href="(elmah_root || '/elmah') + '/xml?id=' + id"
              title="Download as XML"
            >
              <span>xml</span>
            </a>
            <a
              type="button"
              class="btn btn-m btn-outline-info"
              target="_blank"
              :href="(elmah_root || '/elmah') + '/json?id=' + id"
              title="Download as JSON"
            >
              <span>json</span>
            </a>
            <a
              type="button"
              class="btn btn-m btn-outline-info"
              target="_blank"
              :href="(elmah_root || '/elmah') + '/detail/' + id"
              title="Open in new window"
            >
              <font-awesome-icon icon="external-link-alt" />
            </a>
          </div>
          <div class="item-header">
            <div class="item-subheader">
              <div class="filter-link-hid">
                <div class="status" :class="[item.severity]">
                  {{ item.statusCode }}
                </div>
                <b-link
                  class="filter-link"
                  @click="addFilter('status-code', '=', item.statusCode)"
                  style="
                    position: absolute;
                    margin-left: 35px;
                    margin-top: -3px;
                  "
                >
                  <font-awesome-icon icon="filter" class="mr-sm-2" />
                </b-link>
              </div>
              <div>
                <h3 class="message filter-link-hid">
                  {{ item.message }}
                  <b-link
                    class="filter-link"
                    @click="addFilter('message', '=', item.message)"
                  >
                    <font-awesome-icon icon="filter" />
                  </b-link>
                </h3>
                <h6 class="text-info filter-link-hid">
                  {{ item.type }}
                  <b-link
                    class="filter-link"
                    @click="addFilter('type', '=', item.type)"
                  >
                    <font-awesome-icon icon="filter" />
                  </b-link>
                </h6>
              </div>
            </div>
          </div>
        </div>
        <div class="item-details">
          <div class="item-details-tab">
            <table>
              <tr>
                <th>Date/Time</th>
                <td>
                  <span class="filter-link-hid">
                    {{ (item.time || "").substring(0, 10) }}
                    <b-link
                      class="filter-link"
                      @click="
                        addFilter('date-time', '~', item.time.substring(0, 10))
                      "
                    >
                      <font-awesome-icon icon="filter" class="mr-sm-2" />
                    </b-link>
                  </span>
                  <span class="filter-link-hid">
                    {{ (item.time || "").substring(11, 11 + 8) }}
                    <b-link
                      class="filter-link"
                      @click="
                        addFilter(
                          'date-time',
                          '=',
                          item.time.substring(0, 10) +
                            ' ' +
                            item.time.substring(11, 11 + 8)
                        )
                      "
                    >
                      <font-awesome-icon icon="filter" class="mr-sm-2" />
                    </b-link>
                  </span>
                </td>
              </tr>
              <tr>
                <th>URL</th>
                <td class="filter-link-hid">
                  <template v-if="item.url">
                    <span class="method">{{ item.method }}</span
                    ><a target="_blank" v-bind:href="item.url"
                      >{{ item.url }}
                      <font-awesome-icon icon="external-link-alt"
                    /></a>
                  </template>
                  <b-link
                    class="filter-link"
                    @click="addFilter('url', '=', item.url)"
                  >
                    <font-awesome-icon icon="filter" class="mr-sm-2" />
                  </b-link>
                </td>
              </tr>
              <tr>
                <th>Host Name</th>
                <td class="filter-link-hid">
                  {{ item.hostName }}
                  <b-link
                    class="filter-link"
                    @click="addFilter('hostName', '=', item.hostName)"
                  >
                    <font-awesome-icon icon="filter" class="mr-sm-2" />
                  </b-link>
                </td>
              </tr>
              <tr>
                <th>Client IP</th>
                <td class="filter-link-hid">
                  <flag
                    v-if="countryInfo.countryCode"
                    :iso="countryInfo.countryCode"
                    :title="countryInfo.country || countryInfo.countryCode"
                  />
                  <a
                    v-if="item.client"
                    target="_blank"
                    v-bind:href="'https://db-ip.com/' + item.client"
                    >{{ item.client }}
                    <font-awesome-icon icon="external-link-alt"
                  /></a>
                  <b-link
                    class="filter-link"
                    @click="addFilter('client', '=', item.client)"
                  >
                    <font-awesome-icon icon="filter" class="mr-sm-2" />
                  </b-link>
                </td>
              </tr>
            </table>
            <table>
              <tr>
                <th>Application</th>
                <td class="filter-link-hid">
                  {{ item.applicationName }}
                  <b-link
                    class="filter-link"
                    @click="addFilter('application', '=', item.applicationName)"
                  >
                    <font-awesome-icon icon="filter" class="mr-sm-2" />
                  </b-link>
                </td>
              </tr>
              <tr>
                <th>Source</th>
                <td class="filter-link-hid">
                  {{ item.source }}
                  <b-link
                    class="filter-link"
                    @click="addFilter('source', '=', item.source)"
                  >
                    <font-awesome-icon icon="filter" class="mr-sm-2" />
                  </b-link>
                </td>
              </tr>
              <tr>
                <th>User</th>
                <td class="filter-link-hid">
                  {{ item.user }}
                  <b-link
                    class="filter-link"
                    @click="addFilter('user', '=', item.user)"
                  >
                    <font-awesome-icon icon="filter" class="mr-sm-2" />
                  </b-link>
                </td>
              </tr>
            </table>
          </div>
          <div class="spacer"></div>
          <div class="request-info">
            <div class="os">
              <svg
                v-if="item.os === 'Windows'"
                version="1.1"
                xmlns="http://www.w3.org/2000/svg"
                xmlns:xlink="http://www.w3.org/1999/xlink"
                x="0px"
                y="0px"
                width="50px"
                height="60px"
                viewBox="0 0 305 305"
                style="enable-background: new 0 0 305 305"
                xml:space="preserve"
              >
                <g id="XMLID_108_">
                  <path
                    id="XMLID_109_"
                    d="M139.999,25.775v116.724c0,1.381,1.119,2.5,2.5,2.5H302.46c1.381,0,2.5-1.119,2.5-2.5V2.5
                                        c0-0.726-0.315-1.416-0.864-1.891c-0.548-0.475-1.275-0.687-1.996-0.583L142.139,23.301
                                        C140.91,23.48,139.999,24.534,139.999,25.775z"
                  />
                  <path
                    id="XMLID_110_"
                    d="M122.501,279.948c0.601,0,1.186-0.216,1.644-0.616c0.544-0.475,0.856-1.162,0.856-1.884V162.5
                                        c0-1.381-1.119-2.5-2.5-2.5H2.592c-0.663,0-1.299,0.263-1.768,0.732c-0.469,0.469-0.732,1.105-0.732,1.768l0.006,98.515
                                        c0,1.25,0.923,2.307,2.16,2.477l119.903,16.434C122.274,279.94,122.388,279.948,122.501,279.948z"
                  />
                  <path
                    id="XMLID_138_"
                    d="M2.609,144.999h119.892c1.381,0,2.5-1.119,2.5-2.5V28.681c0-0.722-0.312-1.408-0.855-1.883
                                        c-0.543-0.475-1.261-0.693-1.981-0.594L2.164,42.5C0.923,42.669-0.001,43.728,0,44.98l0.109,97.521
                                        C0.111,143.881,1.23,144.999,2.609,144.999z"
                  />
                  <path
                    id="XMLID_169_"
                    d="M302.46,305c0.599,0,1.182-0.215,1.64-0.613c0.546-0.475,0.86-1.163,0.86-1.887l0.04-140
                                        c0-0.663-0.263-1.299-0.732-1.768c-0.469-0.469-1.105-0.732-1.768-0.732H142.499c-1.381,0-2.5,1.119-2.5,2.5v117.496
                                        c0,1.246,0.918,2.302,2.151,2.476l159.961,22.504C302.228,304.992,302.344,305,302.46,305z"
                  />
                </g>
              </svg>
              <svg
                v-if="item.os === 'Linux'"
                version="1.1"
                id="Layer_1"
                xmlns="http://www.w3.org/2000/svg"
                xmlns:xlink="http://www.w3.org/1999/xlink"
                x="0px"
                y="0px"
                width="50px"
                height="60px"
                viewBox="0 0 304.998 304.998"
                style="enable-background: new 0 0 304.998 304.998"
                xml:space="preserve"
              >
                <g id="XMLID_91_">
                  <path
                    id="XMLID_92_"
                    d="M274.659,244.888c-8.944-3.663-12.77-8.524-12.4-15.777c0.381-8.466-4.422-14.667-6.703-17.117
                                        c1.378-5.264,5.405-23.474,0.004-39.291c-5.804-16.93-23.524-42.787-41.808-68.204c-7.485-10.438-7.839-21.784-8.248-34.922
                                        c-0.392-12.531-0.834-26.735-7.822-42.525C190.084,9.859,174.838,0,155.851,0c-11.295,0-22.889,3.53-31.811,9.684
                                        c-18.27,12.609-15.855,40.1-14.257,58.291c0.219,2.491,0.425,4.844,0.545,6.853c1.064,17.816,0.096,27.206-1.17,30.06
                                        c-0.819,1.865-4.851,7.173-9.118,12.793c-4.413,5.812-9.416,12.4-13.517,18.539c-4.893,7.387-8.843,18.678-12.663,29.597
                                        c-2.795,7.99-5.435,15.537-8.005,20.047c-4.871,8.676-3.659,16.766-2.647,20.505c-1.844,1.281-4.508,3.803-6.757,8.557
                                        c-2.718,5.8-8.233,8.917-19.701,11.122c-5.27,1.078-8.904,3.294-10.804,6.586c-2.765,4.791-1.259,10.811,0.115,14.925
                                        c2.03,6.048,0.765,9.876-1.535,16.826c-0.53,1.604-1.131,3.42-1.74,5.423c-0.959,3.161-0.613,6.035,1.026,8.542
                                        c4.331,6.621,16.969,8.956,29.979,10.492c7.768,0.922,16.27,4.029,24.493,7.035c8.057,2.944,16.388,5.989,23.961,6.913
                                        c1.151,0.145,2.291,0.218,3.39,0.218c11.434,0,16.6-7.587,18.238-10.704c4.107-0.838,18.272-3.522,32.871-3.882
                                        c14.576-0.416,28.679,2.462,32.674,3.357c1.256,2.404,4.567,7.895,9.845,10.724c2.901,1.586,6.938,2.495,11.073,2.495
                                        c0.001,0,0,0,0.001,0c4.416,0,12.817-1.044,19.466-8.039c6.632-7.028,23.202-16,35.302-22.551c2.7-1.462,5.226-2.83,7.441-4.065
                                        c6.797-3.768,10.506-9.152,10.175-14.771C282.445,250.905,279.356,246.811,274.659,244.888z M124.189,243.535
                                        c-0.846-5.96-8.513-11.871-17.392-18.715c-7.26-5.597-15.489-11.94-17.756-17.312c-4.685-11.082-0.992-30.568,5.447-40.602
                                        c3.182-5.024,5.781-12.643,8.295-20.011c2.714-7.956,5.521-16.182,8.66-19.783c4.971-5.622,9.565-16.561,10.379-25.182
                                        c4.655,4.444,11.876,10.083,18.547,10.083c1.027,0,2.024-0.134,2.977-0.403c4.564-1.318,11.277-5.197,17.769-8.947
                                        c5.597-3.234,12.499-7.222,15.096-7.585c4.453,6.394,30.328,63.655,32.972,82.044c2.092,14.55-0.118,26.578-1.229,31.289
                                        c-0.894-0.122-1.96-0.221-3.08-0.221c-7.207,0-9.115,3.934-9.612,6.283c-1.278,6.103-1.413,25.618-1.427,30.003
                                        c-2.606,3.311-15.785,18.903-34.706,21.706c-7.707,1.12-14.904,1.688-21.39,1.688c-5.544,0-9.082-0.428-10.551-0.651l-9.508-10.879
                                        C121.429,254.489,125.177,250.583,124.189,243.535z M136.254,64.149c-0.297,0.128-0.589,0.265-0.876,0.411
                                        c-0.029-0.644-0.096-1.297-0.199-1.952c-1.038-5.975-5-10.312-9.419-10.312c-0.327,0-0.656,0.025-1.017,0.08
                                        c-2.629,0.438-4.691,2.413-5.821,5.213c0.991-6.144,4.472-10.693,8.602-10.693c4.85,0,8.947,6.536,8.947,14.272
                                        C136.471,62.143,136.4,63.113,136.254,64.149z M173.94,68.756c0.444-1.414,0.684-2.944,0.684-4.532
                                        c0-7.014-4.45-12.509-10.131-12.509c-5.552,0-10.069,5.611-10.069,12.509c0,0.47,0.023,0.941,0.067,1.411
                                        c-0.294-0.113-0.581-0.223-0.861-0.329c-0.639-1.935-0.962-3.954-0.962-6.015c0-8.387,5.36-15.211,11.95-15.211
                                        c6.589,0,11.95,6.824,11.95,15.211C176.568,62.78,175.605,66.11,173.94,68.756z M169.081,85.08
                                        c-0.095,0.424-0.297,0.612-2.531,1.774c-1.128,0.587-2.532,1.318-4.289,2.388l-1.174,0.711c-4.718,2.86-15.765,9.559-18.764,9.952
                                        c-2.037,0.274-3.297-0.516-6.13-2.441c-0.639-0.435-1.319-0.897-2.044-1.362c-5.107-3.351-8.392-7.042-8.763-8.485
                                        c1.665-1.287,5.792-4.508,7.905-6.415c4.289-3.988,8.605-6.668,10.741-6.668c0.113,0,0.215,0.008,0.321,0.028
                                        c2.51,0.443,8.701,2.914,13.223,4.718c2.09,0.834,3.895,1.554,5.165,2.01C166.742,82.664,168.828,84.422,169.081,85.08z
                                         M205.028,271.45c2.257-10.181,4.857-24.031,4.436-32.196c-0.097-1.855-0.261-3.874-0.42-5.826
                                        c-0.297-3.65-0.738-9.075-0.283-10.684c0.09-0.042,0.19-0.078,0.301-0.109c0.019,4.668,1.033,13.979,8.479,17.226
                                        c2.219,0.968,4.755,1.458,7.537,1.458c7.459,0,15.735-3.659,19.125-7.049c1.996-1.996,3.675-4.438,4.851-6.372
                                        c0.257,0.753,0.415,1.737,0.332,3.005c-0.443,6.885,2.903,16.019,9.271,19.385l0.927,0.487c2.268,1.19,8.292,4.353,8.389,5.853
                                        c-0.001,0.001-0.051,0.177-0.387,0.489c-1.509,1.379-6.82,4.091-11.956,6.714c-9.111,4.652-19.438,9.925-24.076,14.803
                                        c-6.53,6.872-13.916,11.488-18.376,11.488c-0.537,0-1.026-0.068-1.461-0.206C206.873,288.406,202.886,281.417,205.028,271.45z
                                         M39.917,245.477c-0.494-2.312-0.884-4.137-0.465-5.905c0.304-1.31,6.771-2.714,9.533-3.313c3.883-0.843,7.899-1.714,10.525-3.308
                                        c3.551-2.151,5.474-6.118,7.17-9.618c1.228-2.531,2.496-5.148,4.005-6.007c0.085-0.05,0.215-0.108,0.463-0.108
                                        c2.827,0,8.759,5.943,12.177,11.262c0.867,1.341,2.473,4.028,4.331,7.139c5.557,9.298,13.166,22.033,17.14,26.301
                                        c3.581,3.837,9.378,11.214,7.952,17.541c-1.044,4.909-6.602,8.901-7.913,9.784c-0.476,0.108-1.065,0.163-1.758,0.163
                                        c-7.606,0-22.662-6.328-30.751-9.728l-1.197-0.503c-4.517-1.894-11.891-3.087-19.022-4.241c-5.674-0.919-13.444-2.176-14.732-3.312
                                        c-1.044-1.171,0.167-4.978,1.235-8.337c0.769-2.414,1.563-4.91,1.998-7.523C41.225,251.596,40.499,248.203,39.917,245.477z"
                  />
                </g>
              </svg>
              <svg
                v-if="item.os === 'Macintosh'"
                version="1.1"
                viewBox="0 0 42 42"
                xmlns="http://www.w3.org/2000/svg"
                x="0px"
                y="0px"
                width="50px"
                height="60px"
              >
                <path
                  d="m23.091 14.018v-0.342l-1.063 0.073c-0.301 0.019-0.527 0.083-0.679 0.191-0.152 0.109-0.228 0.26-0.228 0.453 0 0.188 0.075 0.338 0.226 0.449 0.15 0.112 0.352 0.167 0.604 0.167 0.161 0 0.312-0.025 0.451-0.074s0.261-0.118 0.363-0.206c0.102-0.087 0.182-0.191 0.239-0.312 0.058-0.121 0.087-0.254 0.087-0.399zm-2.091-13.768c-11.579 0-20.75 9.171-20.75 20.75 0 11.58 9.171 20.75 20.75 20.75s20.75-9.17 20.75-20.75c0-11.579-9.17-20.75-20.75-20.75zm4.028 12.299c0.098-0.275 0.236-0.511 0.415-0.707s0.394-0.347 0.646-0.453 0.533-0.159 0.842-0.159c0.279 0 0.531 0.042 0.755 0.125 0.225 0.083 0.417 0.195 0.578 0.336s0.289 0.305 0.383 0.493 0.15 0.387 0.169 0.596h-0.833c-0.021-0.115-0.059-0.223-0.113-0.322s-0.125-0.185-0.213-0.258c-0.089-0.073-0.193-0.13-0.312-0.171-0.12-0.042-0.254-0.062-0.405-0.062-0.177 0-0.338 0.036-0.481 0.107-0.144 0.071-0.267 0.172-0.369 0.302s-0.181 0.289-0.237 0.475c-0.057 0.187-0.085 0.394-0.085 0.622 0 0.236 0.028 0.448 0.085 0.634 0.056 0.187 0.136 0.344 0.24 0.473 0.103 0.129 0.228 0.228 0.373 0.296s0.305 0.103 0.479 0.103c0.285 0 0.517-0.067 0.697-0.201s0.296-0.33 0.35-0.588h0.834c-0.024 0.228-0.087 0.436-0.189 0.624s-0.234 0.348-0.396 0.481c-0.163 0.133-0.354 0.236-0.574 0.308s-0.462 0.109-0.725 0.109c-0.312 0-0.593-0.052-0.846-0.155-0.252-0.103-0.469-0.252-0.649-0.445s-0.319-0.428-0.417-0.705-0.147-0.588-0.147-0.935c-2e-3 -0.339 0.047-0.647 0.145-0.923zm-11.853-1.262h0.834v0.741h0.016c0.051-0.123 0.118-0.234 0.2-0.33 0.082-0.097 0.176-0.179 0.284-0.248 0.107-0.069 0.226-0.121 0.354-0.157 0.129-0.036 0.265-0.054 0.407-0.054 0.306 0 0.565 0.073 0.775 0.219 0.211 0.146 0.361 0.356 0.449 0.63h0.021c0.056-0.132 0.13-0.25 0.221-0.354s0.196-0.194 0.314-0.268 0.248-0.13 0.389-0.169 0.289-0.058 0.445-0.058c0.215 0 0.41 0.034 0.586 0.103s0.326 0.165 0.451 0.29 0.221 0.277 0.288 0.455 0.101 0.376 0.101 0.594v2.981h-0.87v-2.772c0-0.287-0.074-0.51-0.222-0.667-0.147-0.157-0.358-0.236-0.632-0.236-0.134 0-0.257 0.024-0.369 0.071-0.111 0.047-0.208 0.113-0.288 0.198-0.081 0.084-0.144 0.186-0.189 0.304-0.046 0.118-0.069 0.247-0.069 0.387v2.715h-0.858v-2.844c0-0.126-0.02-0.24-0.059-0.342s-0.094-0.189-0.167-0.262c-0.072-0.073-0.161-0.128-0.264-0.167-0.104-0.039-0.22-0.059-0.349-0.059-0.134 0-0.258 0.025-0.373 0.075-0.114 0.05-0.212 0.119-0.294 0.207-0.082 0.089-0.146 0.193-0.191 0.314-0.044 0.12-0.116 0.252-0.116 0.394v2.683h-0.825v-4.374zm1.893 20.939c-3.825 0-6.224-2.658-6.224-6.9s2.399-6.909 6.224-6.909 6.215 2.667 6.215 6.909c0 4.241-2.39 6.9-6.215 6.9zm7.082-16.575c-0.141 0.036-0.285 0.054-0.433 0.054-0.218 0-0.417-0.031-0.598-0.093-0.182-0.062-0.337-0.149-0.467-0.262s-0.232-0.249-0.304-0.409c-0.073-0.16-0.109-0.338-0.109-0.534 0-0.384 0.143-0.684 0.429-0.9s0.7-0.342 1.243-0.377l1.18-0.068v-0.338c0-0.252-0.08-0.445-0.24-0.576s-0.386-0.197-0.679-0.197c-0.118 0-0.229 0.015-0.331 0.044-0.102 0.03-0.192 0.072-0.27 0.127s-0.143 0.121-0.193 0.198c-0.051 0.076-0.086 0.162-0.105 0.256h-0.818c5e-3 -0.193 0.053-0.372 0.143-0.536s0.212-0.306 0.367-0.427 0.336-0.215 0.546-0.282 0.438-0.101 0.685-0.101c0.266 0 0.507 0.033 0.723 0.101s0.401 0.163 0.554 0.288 0.271 0.275 0.354 0.451 0.125 0.373 0.125 0.59v3.001h-0.833v-0.729h-0.021c-0.062 0.118-0.14 0.225-0.235 0.32-0.096 0.095-0.203 0.177-0.322 0.244-0.12 0.067-0.25 0.119-0.391 0.155zm5.503 16.575c-2.917 0-4.9-1.528-5.038-3.927h1.899c0.148 1.371 1.473 2.279 3.288 2.279 1.741 0 2.992-0.908 2.992-2.149 0-1.074-0.76-1.723-2.519-2.167l-1.714-0.426c-2.464-0.611-3.584-1.732-3.584-3.575 0-2.269 1.982-3.844 4.807-3.844 2.76 0 4.686 1.584 4.76 3.862h-1.88c-0.13-1.371-1.25-2.214-2.918-2.214-1.658 0-2.806 0.852-2.806 2.084 0 0.972 0.722 1.547 2.482 1.991l1.445 0.361c2.751 0.667 3.881 1.751 3.881 3.696-1e-3 2.482-1.964 4.029-5.095 4.029zm-12.585-12.106c-2.621 0-4.26 2.01-4.26 5.205 0 3.186 1.639 5.196 4.26 5.196 2.612 0 4.26-2.01 4.26-5.196 1e-3 -3.195-1.648-5.205-4.26-5.205z"
                />
              </svg>
              <svg
                v-if="item.os === 'iPhone'"
                version="1.1"
                xmlns="http://www.w3.org/2000/svg"
                xmlns:xlink="http://www.w3.org/1999/xlink"
                x="0px"
                y="0px"
                width="50px"
                height="60px"
                viewBox="0 0 22.773 22.773"
                style="enable-background: new 0 0 22.773 22.773"
                xml:space="preserve"
              >
                <g>
                  <g>
                    <path
                      d="M15.769,0c0.053,0,0.106,0,0.162,0c0.13,1.606-0.483,2.806-1.228,3.675c-0.731,0.863-1.732,1.7-3.351,1.573
                                            c-0.108-1.583,0.506-2.694,1.25-3.561C13.292,0.879,14.557,0.16,15.769,0z"
                    />
                    <path
                      d="M20.67,16.716c0,0.016,0,0.03,0,0.045c-0.455,1.378-1.104,2.559-1.896,3.655c-0.723,0.995-1.609,2.334-3.191,2.334
                                            c-1.367,0-2.275-0.879-3.676-0.903c-1.482-0.024-2.297,0.735-3.652,0.926c-0.155,0-0.31,0-0.462,0
                                            c-0.995-0.144-1.798-0.932-2.383-1.642c-1.725-2.098-3.058-4.808-3.306-8.276c0-0.34,0-0.679,0-1.019
                                            c0.105-2.482,1.311-4.5,2.914-5.478c0.846-0.52,2.009-0.963,3.304-0.765c0.555,0.086,1.122,0.276,1.619,0.464
                                            c0.471,0.181,1.06,0.502,1.618,0.485c0.378-0.011,0.754-0.208,1.135-0.347c1.116-0.403,2.21-0.865,3.652-0.648
                                            c1.733,0.262,2.963,1.032,3.723,2.22c-1.466,0.933-2.625,2.339-2.427,4.74C17.818,14.688,19.086,15.964,20.67,16.716z"
                    />
                  </g>
                </g>
              </svg>
              <svg
                v-if="item.os === 'Android'"
                version="1.1"
                xmlns="http://www.w3.org/2000/svg"
                xmlns:xlink="http://www.w3.org/1999/xlink"
                x="0px"
                y="0px"
                width="50px"
                height="60px"
                viewBox="0 0 553.048 553.048"
                style="enable-background: new 0 0 553.048 553.048"
                xml:space="preserve"
              >
                <g>
                  <g>
                    <path
                      d="M76.774,179.141c-9.529,0-17.614,3.323-24.26,9.969c-6.646,6.646-9.97,14.621-9.97,23.929v142.914
                                            c0,9.541,3.323,17.619,9.97,24.266c6.646,6.646,14.731,9.97,24.26,9.97c9.522,0,17.558-3.323,24.101-9.97
                                            c6.53-6.646,9.804-14.725,9.804-24.266V213.039c0-9.309-3.323-17.283-9.97-23.929C94.062,182.464,86.082,179.141,76.774,179.141z"
                    />
                    <path
                      d="M351.972,50.847L375.57,7.315c1.549-2.882,0.998-5.092-1.658-6.646c-2.883-1.34-5.098-0.661-6.646,1.989l-23.928,43.88
                                            c-21.055-9.309-43.324-13.972-66.807-13.972c-23.488,0-45.759,4.664-66.806,13.972l-23.929-43.88
                                            c-1.555-2.65-3.77-3.323-6.646-1.989c-2.662,1.561-3.213,3.764-1.658,6.646l23.599,43.532
                                            c-23.929,12.203-42.987,29.198-57.167,51.022c-14.18,21.836-21.273,45.698-21.273,71.628h307.426
                                            c0-25.924-7.094-49.787-21.273-71.628C394.623,80.045,375.675,63.05,351.972,50.847z M215.539,114.165
                                            c-2.552,2.558-5.6,3.831-9.143,3.831c-3.55,0-6.536-1.273-8.972-3.831c-2.436-2.546-3.654-5.582-3.654-9.137
                                            c0-3.543,1.218-6.585,3.654-9.137c2.436-2.546,5.429-3.819,8.972-3.819s6.591,1.273,9.143,3.819
                                            c2.546,2.558,3.825,5.594,3.825,9.137C219.357,108.577,218.079,111.619,215.539,114.165z M355.625,114.165
                                            c-2.441,2.558-5.434,3.831-8.971,3.831c-3.551,0-6.598-1.273-9.145-3.831c-2.551-2.546-3.824-5.582-3.824-9.137
                                            c0-3.543,1.273-6.585,3.824-9.137c2.547-2.546,5.594-3.819,9.145-3.819c3.543,0,6.529,1.273,8.971,3.819
                                            c2.438,2.558,3.654,5.594,3.654,9.137C359.279,108.577,358.062,111.619,355.625,114.165z"
                    />
                    <path
                      d="M123.971,406.804c0,10.202,3.543,18.838,10.63,25.925c7.093,7.087,15.729,10.63,25.924,10.63h24.596l0.337,75.454
                                            c0,9.528,3.323,17.619,9.969,24.266s14.627,9.97,23.929,9.97c9.523,0,17.613-3.323,24.26-9.97s9.97-14.737,9.97-24.266v-75.447
                                            h45.864v75.447c0,9.528,3.322,17.619,9.969,24.266s14.73,9.97,24.26,9.97c9.523,0,17.613-3.323,24.26-9.97
                                            s9.969-14.737,9.969-24.266v-75.447h24.928c9.969,0,18.494-3.544,25.594-10.631c7.086-7.087,10.631-15.723,10.631-25.924V185.45
                                            H123.971V406.804z"
                    />
                    <path
                      d="M476.275,179.141c-9.309,0-17.283,3.274-23.93,9.804c-6.646,6.542-9.969,14.578-9.969,24.094v142.914
                                            c0,9.541,3.322,17.619,9.969,24.266s14.627,9.97,23.93,9.97c9.523,0,17.613-3.323,24.26-9.97s9.969-14.725,9.969-24.266V213.039
                                            c0-9.517-3.322-17.552-9.969-24.094C493.888,182.415,485.798,179.141,476.275,179.141z"
                    />
                  </g>
                </g>
              </svg>
              <div>{{ item.os }}</div>
            </div>
            <div class="browser">
              <svg
                v-if="item.browser === 'Chrome'"
                version="1.1"
                xmlns="http://www.w3.org/2000/svg"
                xmlns:xlink="http://www.w3.org/1999/xlink"
                x="0px"
                y="0px"
                width="50px"
                height="60px"
                viewBox="0 0 305 305"
                style="enable-background: new 0 0 305 305"
                xml:space="preserve"
              >
                <g id="XMLID_16_">
                  <path
                    id="XMLID_17_"
                    d="M95.506,152.511c0,31.426,25.567,56.991,56.994,56.991c31.425,0,56.99-25.566,56.99-56.991
                                        c0-31.426-25.565-56.993-56.99-56.993C121.073,95.518,95.506,121.085,95.506,152.511z"
                  />
                  <path
                    id="XMLID_18_"
                    d="M283.733,77.281c0.444-0.781,0.436-1.74-0.023-2.513c-13.275-22.358-32.167-41.086-54.633-54.159
                                        C205.922,7.134,179.441,0.012,152.5,0.012c-46.625,0-90.077,20.924-119.215,57.407c-0.643,0.804-0.727,1.919-0.212,2.81
                                        l42.93,74.355c0.45,0.78,1.28,1.25,2.164,1.25c0.112,0,0.226-0.008,0.339-0.023c1.006-0.137,1.829-0.869,2.083-1.852
                                        c8.465-32.799,38.036-55.706,71.911-55.706c2.102,0,4.273,0.096,6.455,0.282c0.071,0.007,0.143,0.01,0.214,0.01H281.56
                                        C282.459,78.545,283.289,78.063,283.733,77.281z"
                  />
                  <path
                    id="XMLID_19_"
                    d="M175.035,224.936c-0.621-0.803-1.663-1.148-2.646-0.876c-6.457,1.798-13.148,2.709-19.889,2.709
                                        c-28.641,0-55.038-16.798-67.251-42.794c-0.03-0.064-0.063-0.126-0.098-0.188L23.911,77.719c-0.446-0.775-1.272-1.25-2.165-1.25
                                        c-0.004,0-0.009,0-0.013,0c-0.898,0.005-1.725,0.49-2.165,1.272C6.767,100.456,0,126.311,0,152.511
                                        c0,36.755,13.26,72.258,37.337,99.969c23.838,27.435,56.656,45.49,92.411,50.84c0.124,0.019,0.248,0.027,0.371,0.027
                                        c0.883,0,1.713-0.47,2.164-1.25l42.941-74.378C175.732,226.839,175.657,225.739,175.035,224.936z"
                  />
                  <path
                    id="XMLID_20_"
                    d="M292.175,95.226h-85.974c-1.016,0-1.931,0.615-2.314,1.555c-0.384,0.94-0.161,2.02,0.564,2.73
                                        c14.385,14.102,22.307,32.924,22.307,53c0,15.198-4.586,29.824-13.263,42.298c-0.04,0.058-0.077,0.117-0.112,0.178l-61.346,106.252
                                        c-0.449,0.778-0.446,1.737,0.007,2.513c0.449,0.767,1.271,1.237,2.158,1.237c0.009,0,0.019,0,0.028,0
                                        c40.37-0.45,78.253-16.511,106.669-45.222C289.338,231.032,305,192.941,305,152.511c0-19.217-3.532-37.956-10.498-55.698
                                        C294.126,95.855,293.203,95.226,292.175,95.226z"
                  />
                </g>
              </svg>
              <div>{{ item.browser }}</div>
            </div>
          </div>
        </div>
      </div>
      <div
        class="col-xs-12 alert alert-success msdn"
        v-if="helpHtml && helpHtml.html"
      >
        <span class="mr-sm-2 mt-sm-2">
          <font-awesome-icon icon="info-circle" />
        </span>
        <div>
          <div v-html="helpHtml.html"></div>
          <div>
            <span class="see-more">See more: </span>
            <a target="_blank" :href="helpHtml.path">{{
              helpHtml.path.indexOf("mozilla") > -1 ? "MDN" : "MSDN"
            }}</a>
            |
            <a
              target="_blank"
              :href="
                'https://stackoverflow.com/search?q=' +
                item.type +
                ' ' +
                item.message
              "
              >Stack Overflow</a
            >
            |
            <a
              target="_blank"
              :href="
                'https://google.com/search?q=' + item.type + ' ' + item.message
              "
              >Google</a
            >
          </div>
        </div>
      </div>
    </div>
    <div class="item-additions">
      <b-tabs v-model="selectedTab">
        <b-tab
          v-if="item.sources && item.sources.length > 0"
          title="Source Code"
        >
          <template v-for="(code, key) in item.sources">
            <div v-bind:key="key" class="sources">
              <div class="source-line">
                at <span class="type">{{ code.type }}</span
                >.<span class="method">{{ code.function }}</span
                >() in <span class="filename">{{ code.fileName }}</span
                >(<span class="line">{{ code.line }}</span
                >)
              </div>
              <highlight-code class="src-code" lang="csharp">
                {{ code.preContextCode + "\n" }}
                {{
                  code.contextCode +
                  "// Error in this line at " +
                  code.fileName +
                  " (" +
                  code.line +
                  ") \n "
                }}
                {{ code.postContextCode + "\n" }}
              </highlight-code>
            </div>
          </template>
        </b-tab>
        <b-tab key="stack" v-if="item.htmlMessage" title="Stack Trace" lazy>
          <pre class="item-stack" v-html="item.htmlMessage"></pre>
        </b-tab>
        <b-tab key="body" v-if="item.body" title="Request Body" lazy>
          <highlight-code class="src-code" lang="json">
            {{ body }}
          </highlight-code>
        </b-tab>
        <b-tab v-if="item.messageLog && item.messageLog.length > 0" lazy>
          <template #title>
            <span class="count">{{ item.messageLog.length }}</span
            >Log
          </template>
          <div
            class="log-entry"
            v-for="entry in item.messageLog"
            v-bind:key="entry.timeStamp"
          >
            <div class="log-row">
              <div class="time-stamp">
                {{ entry.timeStamp | moment("HH:mm:ss.SSS") }}
              </div>
              <div class="level" :class="[levelString(entry.level)]">
                {{ levelString(entry.level) }}
              </div>
              <div class="message">
                {{ entry.message }}
                <a
                  v-if="entry.exception || entry.scope"
                  v-on:click.prevent="entry.collapsed = !entry.collapsed"
                  href="#"
                  >{{ entry.collapsed ? "more..." : "hide details" }}</a
                >
                <a
                  v-if="entry.params && entry.params.length > 0"
                  v-on:click.prevent="entry.collapsed = !entry.collapsed"
                  href="#"
                  >{{ entry.collapsed ? "params..." : "hide params" }}</a
                >
              </div>
            </div>
            <div
              v-if="entry.params && entry.params.length > 0 && !entry.collapsed"
              class="params"
            >
              <template v-for="p in entry.params">
                <label :key="p.timeStamp">{{ p.key }}:</label>
                <highlight-code lang="json" :key="p.timeStamp">{{
                  p.value
                }}</highlight-code>
              </template>
            </div>
            <div v-if="entry.exception && !entry.collapsed" class="exception">
              <label>Exception</label>
              <span>{{ entry.exception }}</span>
            </div>
            <div v-if="entry.scope && !entry.collapsed" class="scope">
              <label>Scope</label>
              <span>{{ entry.scope }}</span>
            </div>
          </div>
        </b-tab>
        <b-tab key="sqlLog" v-if="item.sqlLog && item.sqlLog.length > 0" lazy>
          <template #title>
            <span class="count">{{ item.sqlLog.length }}</span
            >SQL
          </template>
          <div v-for="sqlItem in item.sqlLog" :key="sqlItem.timeStamp">
            <div>
              <span>{{ sqlItem.timeStamp | moment("HH:mm:ss.SSS") }}</span>
              <span>({{ sqlItem.durationMs }} ms)</span>
            </div>
            <highlight-code class="src-code" lang="sql">
              {{ sqlItem.sqlText }}
            </highlight-code>
          </div>
        </b-tab>
        <template
          v-for="(val, name) in {
            'Query String': item.queryString,
            Form: item.form,
            Header: item.header,
            Cookies: item.cookies,
            Connection: item.connection,
            'Server Variables': item.serverVariables,
          }"
        >
          <b-tab
            v-if="val && Object.keys(val).length > 0"
            v-bind:key="name"
            lazy
          >
            <template #title>
              <span class="count">{{ Object.keys(val).length }}</span
              >{{ name }}
            </template>
            <table>
              <tr
                v-for="(value, propertyName) in val"
                v-bind:key="propertyName"
              >
                <th>
                  {{ propertyName }}
                  <b-link
                    v-if="
                      propertyName == ':method' ||
                      propertyName == 'method' ||
                      propertyName == 'host'
                    "
                    class="filter-link"
                    @click="
                      addFilter(propertyName.replace(':', ''), '=', value)
                    "
                  >
                    <font-awesome-icon icon="filter" class="mr-sm-2" />
                  </b-link>
                </th>
                <td>{{ value }}</td>
              </tr>
            </table>
          </b-tab>
        </template>
      </b-tabs>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "ErrorDetail",
  data: function () {
    return {
      collapsed: true,
      countryInfo: {},
      htmlStack: "",
      selectedTab: 0,
      helpHtml: {},
      isMobile: window.innerWidth <= 1024,
    };
  },
  props: {
    item: { type: Object, default: () => ({}) },
    log: { type: Object, default: () => ({}) },
    id: { type: String, default: () => "" },
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
      } catch {
        //ignore
      }
      return this.item.body;
    },
  },
  created() {
    window.addEventListener("resize", this.handleResize);
  },
  destroyed() {
    window.removeEventListener("resize", this.handleResize);
  },
  updated: function () {
    const els = document.getElementsByClassName("src-code");
    if (!els || els.length === 0) return;
    for (let j = 0; j < els.length; j++) {
      let el = els[j];
      if (el) {
        let children = el.children[0].children;
        if (children) {
          for (let i = children.length - 1; i > 0; i--) {
            let ch = children[i];
            if (ch.innerText.indexOf("// Error in this line ") > -1) {
              ch.style.backgroundColor = "#ef4c4c";
              ch.style.color = "#fff";
              ch.style.fontWeight = 600;
              break;
            }
          }
        }
      }
    }
    this.handleResize();
  },
  watch: {
    item: function () {
      this.helpHtml = null;
      if (
        (this.item.type || this.item.statusCode == 0) &&
        this.item.type.toLowerCase() !== "http"
      ) {
        axios
          .get(this.elmah_root + "/exception/" + this.item.type)
          .then((response) => (this.helpHtml = response.data))
          .catch((e) => console.log(e));
      } else {
        axios
          .get(this.elmah_root + "/status/" + this.item.statusCode)
          .then((response) => (this.helpHtml = response.data))
          .catch((e) => console.log(e));
      }
      if (this.item.client && !this.item.client.startsWith(":")) {
        axios
          .get("http://ip-api.com/json/" + this.item.client)
          .then((response) => (this.countryInfo = response.data))
          .catch((e) => console.log(e));
      }

      let ctx = this;
      setTimeout(() => (ctx.selectedTab = 0), 1);
    },
  },
  methods: {
    addFilter(property, condition, value) {
      this.$root.$refs.ErrorListFilter.addFilterTag(
        property + " " + condition + " " + ("" + value).trim()
      );
    },
    handleResize() {
      this.isMobile = window.innerWidth <= 1024;
      const tabs = document.getElementsByClassName("tab-content");
      if (!tabs) return;
      const height = window.innerHeight - tabs[0].offsetTop - 10;
      if (window.innerWidth <= 1024) {
        tabs[0].style.height = "auto";
      } else {
        tabs[0].style.height = height + "px";
      }
    },
    levelString: function (level) {
      switch (level) {
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
    copyTextToClipboard: function copyTextToClipboard() {
      let bvToast = this.$bvToast;

      let text = `URL: (${this.item.method}) ${this.item.url}
Hostname: ${this.item.hostName}
Status Code: ${this.item.statusCode}
Type: ${this.item.type}
Message: ${this.item.message}
Source: ${this.item.source}
User: ${this.item.user}
Client IP: ${this.item.client}
Time: ${this.item.time}
Detail:
${this.item.detail}
`;
      if (this.item.header && Object.keys(this.item.header).length !== 0) {
        text += "\nHeader:\n";
        let item = this.item;
        text += Object.keys(this.item.header)
          .map(function (k) {
            return "\t" + k + ":" + item.header[k];
          })
          .join("\n");
      }

      if (this.item.cookies && Object.keys(this.item.cookies).length !== 0) {
        text += "\nCookies:\n";
        let item = this.item;
        text += Object.keys(this.item.cookies)
          .map(function (k) {
            return "\t" + k + ":" + item.cookies[k];
          })
          .join("\n");
      }

      if (this.item.form && Object.keys(this.item.form).length !== 0) {
        text += "\nForm:\n";
        let item = this.item;
        text += Object.keys(this.item.form)
          .map(function (k) {
            return "\t" + k + ":" + item.form[k];
          })
          .join("\n");
      }

      if (
        this.item.connection &&
        Object.keys(this.item.connection).length !== 0
      ) {
        text += "\nConnection:\n";
        let item = this.item;
        text += Object.keys(this.item.connection)
          .map(function (k) {
            return "\t" + k + ":" + item.connection[k];
          })
          .join("\n");
      }

      if (this.item.userData && Object.keys(this.item.userData).length !== 0) {
        text += "\nUser Data:\n";
        let item = this.item;
        text += Object.keys(this.item.userData)
          .map(function (k) {
            return "\t" + k + ":" + item.userData[k];
          })
          .join("\n");
      }

      if (this.item.session && Object.keys(this.item.session).length !== 0) {
        text += "\nSession:\n";
        let item = this.item;
        text += Object.keys(this.item.session)
          .map(function (k) {
            return "\t" + k + ":" + item.session[k];
          })
          .join("\n");
      }

      if (
        this.item.serverVariables &&
        Object.keys(this.item.serverVariables).length !== 0
      ) {
        text += "\nServer Variables:\n";
        let item = this.item;
        text += Object.keys(this.item.serverVariables)
          .map(function (k) {
            return "\t" + k + ":" + item.serverVariables[k];
          })
          .join("\n");
      }
      function fallbackCopyTextToClipboard(text) {
        var textArea = document.createElement("textarea");
        textArea.value = text;

        // Avoid scrolling to bottom
        textArea.style.top = "0";
        textArea.style.left = "0";
        textArea.style.position = "fixed";

        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();

        try {
          document.execCommand("copy");
          bvToast.toast("Error copied to clipboard.", {
            variant: "success",
            solid: true,
            noCloseButton: true,
            autoHideDelay: 2000,
          });
        } catch (err) {
          console.error("Fallback: Oops, unable to copy", err);
        }

        document.body.removeChild(textArea);
      }

      if (!navigator.clipboard) {
        fallbackCopyTextToClipboard(text);
        return;
      }
      navigator.clipboard.writeText(text).then(
        function () {
          bvToast.toast("Error copied to clipboard.", {
            variant: "success",
            solid: true,
            noCloseButton: true,
            autoHideDelay: 2000,
          });
        },
        function (err) {
          console.error("Async: Could not copy text: ", err);
        }
      );
    },
  },
};
</script>

<style lang="scss">
@import "src/styles/variables";
.is-mobile {
  overflow-y: scroll;
}

.item-details-tab .filter-link {
  margin-left: 5px;
}

.filter-link-hid .filter-link {
  visibility: hidden;
}

.filter-link-hid:hover .filter-link {
  visibility: visible;
}

.filter-link svg path {
  fill: #17a2b8;
}

pre {
  overflow-wrap: anywhere;
  white-space: pre-wrap; /* Since CSS 2.1 */
  white-space: -moz-pre-wrap; /* Mozilla, since 1999 */
  white-space: -pre-wrap; /* Opera 4-6 */
  white-space: -o-pre-wrap; /* Opera 7 */
  word-wrap: break-word; /* Internet Explorer 5.5+ */
}
span.error-line {
  color: red;
  font-weight: 600;
}
span.comment {
  color: green;
}
.st-type,
.st-param-type {
  color: #00008b;
}

.st-param-name {
  color: #666;
}

.st-method {
  color: #008b8b;
  font-weight: bolder;
}

.st-file,
.st-line {
  color: #8b008b;
}
.nav-tabs .nav-link {
  color: #444;
}
.nav-tabs .nav-link.active {
  color: #000;
  font-weight: 600;
}
.nav.nav-tabs {
  padding-left: 10px;
}
.tab-pane.active {
  height: 100%;
  overflow-y: auto;
  overflow-x: hidden;
  overflow-wrap: anywhere;
}
.msdn {
  .see-more {
    font-style: italic;
    color: #000;
  }
  p {
    margin-bottom: 4px;
  }
}
.e-detail {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  background-color: #faf9f8;

  .item-info {
    display: flex;
    flex-direction: column;
    flex-shrink: 0;

    .item-info-panel {
      flex-grow: 1;

      > div {
        display: flex;
      }

      .item-header {
        display: flex;
        flex-direction: column;
        padding: 10px;

        h4 {
          font-size: 18px;
          flex-grow: 1;
        }

        .item-subheader {
          display: flex;
          flex-direction: row;

          .status {
            background-color: #888;
            color: #fff;
            width: 48px;
            height: 48px;
            min-width: 48px;
            line-height: 48px;
            font-size: 22px;
            padding: 2px -1px;
            border-radius: 48px;
            text-align: center;
            margin-right: 10px;
            margin-top: 5px;
          }

          .status.Error {
            background-color: $error-color;
          }

          .status.Warning {
            background-color: $warning-color;
          }

          .status.Success {
            background-color: $success-color;
          }
        }
      }

      .item-details {
        background-color: #d1ecf1;
        display: flex;
        flex-direction: row;
        align-items: start;
        padding: 10px;
        font-size: 14px;
        margin: 0 10px;
        margin-bottom: 10px;

        .item-details-tab {
          display: flex;
          flex-direction: row;
        }

        .request-info {
          display: flex;
          flex-direction: row;
        }

        .spacer {
          flex-grow: 1;
        }
        svg path {
          fill: #17a2b8;
        }
        .browser,
        .os {
          flex-shrink: 0;
          display: flex;
          flex-direction: column;
          margin-right: 10px;
          font-weight: 500;
          color: #17a2b8;
          align-items: center;
        }

        table {
          margin-right: 40px;
          flex-shrink: 0;
          line-height: 24px;

          th {
            min-width: 80px;
          }
        }

        .flag-icon {
          margin-right: 4px;
          font-size: 15px;
        }

        .method {
          font-size: 9px;
          padding: 2px 5px;
          border: 1px solid #6c757d;
          border-radius: 4px;
          color: #fff;
          background-color: #6c757d;
          font-weight: 400;
          margin-right: 5px;
        }
      }
    }
  }
  .msdn {
    background: #d4edda;
    margin: 0 10px 10px;
    padding: 10px;
    font-size: 14px;
    line-height: 16px;
    display: flex;
    flex-direction: row;

    .fa {
      float: left;
      margin-right: 9px;
      font-size: 32px;
    }
  }
  .toolbar {
    display: flex;
    flex-direction: row;
    justify-content: flex-end;
    position: absolute;
    right: 0;
    padding: 5px;

    button,
    a {
      margin: 2px;
      padding: 3px 10px;
    }

    span {
      font-size: 11px;
      font-weight: bold;
    }
  }
  .item-additions {
    flex-grow: 1;
    font-size: 14px;

    .tabs {
      margin: 0 10px;

      span.count {
        position: relative;
        top: -8px;
        right: 3px;
        background-color: #888;
        color: #fff;
        display: block;
        width: 15px;
        height: 15px;
        min-width: 15px;
        line-height: 16px;
        font-size: 9px;
        padding: 2px -1px;
        border-radius: 20px;
        text-align: center;
        margin-bottom: -15px;
        margin-right: -15px;
        float: right;
      }
      .nav-link.active span.count {
        display: none;
      }

      .tab-content {
        > div {
          border: 1px solid $border-main-color;
          border-top: 0;
          padding: 10px;
          background-color: #fff;

          table {
            width: 100%;

            tr {
              border-bottom: 1px solid #f0f0f0;

              td {
                width: 100%;
                word-break: break-all;
              }
            }

            th {
              padding: 2px;
              width: 100px;
              padding-right: 30px;
              white-space: nowrap;
            }
          }

          pre {
            padding: 15px;
            font-size: 14px;
            background: #f0f0f0;
            font-family: SFMono-Regular, Menlo, Monaco, Consolas,
              "Liberation Mono", "Courier New", monospace;
            overflow-wrap: anywhere;
            white-space: pre-wrap;
          }

          .sources {
            .source-line {
              margin-bottom: 3px;

              .type {
                color: #444;
              }
              .method {
                color: #880000;
                font-weight: 500;
              }
              .filename {
                font-weight: 600;
              }
            }
          }

          .log-entry {
            font-size: 14px;
            .params {
              display: flex;
              flex-direction: column;
            }
            .scope,
            .exception {
              display: flex;
              color: #6c757d;

              span {
                flex-grow: 1;
              }

              label {
                flex-basis: 75px;
                margin-left: 95px;
                font-style: italic;
                flex-shrink: 0;
              }
            }

            .log-row {
              display: flex;
              flex-direction: row;
              overflow-wrap: break-word;

              .time-stamp {
                color: #6c757d;
                flex-basis: 95px;
                flex-shrink: 0;
              }

              .message {
                flex-grow: 1;
              }

              .level {
                flex-basis: 75px;
                flex-shrink: 0;
                color: #6c757d;
                font-weight: bold;
                margin-right: 2px;
                padding: 1px 3px;

                &.Trace {
                  color: #000;
                }

                &.Information {
                  color: #1c7430;
                }

                &.Debug {
                  color: #005cbf;
                }

                &.Error {
                  color: #dc3545;
                }

                &.Warning {
                  color: #bb9a00;
                }

                &.Critical {
                  color: #fff;
                  background-color: $error-color;
                }
              }
            }
          }
        }
      }
    }
  }
}
@media screen and (max-width: 1380px) {
  .e-detail {
    .item-info {
      .item-info-panel {
        > div {
          flex-direction: column-reverse;
        }

        .toolbar {
          align-items: end;
          display: block;
          margin: 0 10px;
          position: relative;
        }

        .item-details {
          font-size: 13px;
          line-height: 16px;

          .item-details-tab {
            flex-direction: column;
          }

          .request-info {
            flex-direction: column;
          }
        }
      }
    }

    .item-additions {
      font-size: 13px;
    }
  }
  .item-additions .tabs .tab-content > div .log-entry * {
    font-size: 13px;
    overflow-wrap: anywhere;
  }
  pre code {
    font-size: 13px !important;
  }

  @media screen and (max-width: 1024px) {
    .col-xs-12.alert.alert-success.msdn {
      display: none;
    }
    .e-detail .item-info .item-info-panel .item-details .request-info {
      display: none;
    }
  }
  @media screen and (max-width: 720px) {
    h3.message {
      font-size: 20px;
    }
    .tabs ul {
      display: block;
      padding-right: 10px;
      padding-bottom: 5px;
    }
    span.count {
      width: 24px;
      height: 24px;
      font-size: 13px;
      margin: 0;
      line-height: 25px;
      margin-top: 5px;
    }
    .nav-link.active span.count {
      display: block;
    }
    li.nav-item a.active {
      margin: 0 !important;
      border: 1px solid #dee2e6 !important;
      border-radius: 2px;
    }
  }
}
</style>
