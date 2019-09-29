var $api = new apiUtils.Api(apiUrl + '/v1/administrators/actions/logout');

var $vue = new Vue({
    el: '#main',
    data: {
        pageLoad: false
    },
    methods: {
        logout: function () {
            var $this = this;

            $api.post(null, function (err, res) {
                if (!res.sso) {
                    $this.redirect();
                } else {
                    $this.redirect(res.value);
                }
            });
        },

        redirect: function (url) {
            window.top.location.href = url ? url : 'pageLogin.cshtml';
        }
    }
});

$vue.logout();