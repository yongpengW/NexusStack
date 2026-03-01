var nexusstack = nexusstack || {};
(function () {
    nexusstack.utils = nexusstack.utils || {};

    nexusstack.utils.setCookieValue = function (key, value, expireDate, path) {
        var cookieValue = encodeURIComponent(key) + '=';

        if (value) {
            cookieValue = cookieValue + encodeURIComponent(value);
        }

        if (expireDate) {
            cookieValue = cookieValue + '; expires=' + expireDate.toUTCString();
        }

        if (path) {
            cookieValue = cookieValue + '; path=' + path;
        }

        document.cookie = cookieValue;
    };

    nexusstack.utils.getCookieValue = function (key) {
        var equalities = document.cookie.split('; ');
        for (var i = 0; i < equalities.length; i++) {
            if (!equalities[i]) {
                continue;
            }

            var splitted = equalities[i].split('=');
            if (splitted.length != 2) {
                continue;
            }

            if (decodeURIComponent(splitted[0]) === key) {
                return decodeURIComponent(splitted[1] || '');
            }
        }

        return null;
    };

    nexusstack.auth = nexusstack.auth || {};

    nexusstack.auth.tokenHeaderName = 'Authorization';
    nexusstack.auth.tokenCookieName = 'NexusStack.AuthToken';

    nexusstack.auth.getToken = function () {
        return nexusstack.utils.getCookieValue(nexusstack.auth.tokenCookieName);
    };

    nexusstack.auth.setToken = function (token, expireDate) {
        console.log('SetToken', token, expireDate);
        nexusstack.utils.setCookieValue(nexusstack.auth.tokenCookieName, token, expireDate, '/');
    };

    nexusstack.auth.clearToken = function () {
        nexusstack.auth.setToken();
    };

    nexusstack.auth.requestInterceptor = function (request) {
        var token = nexusstack.auth.getToken();
        request.headers.Authorization = token;

        return request;
    };

    nexusstack.swagger = nexusstack.swagger || {};

    nexusstack.swagger.openAuthDialog = function (loginCallback) {
        nexusstack.swagger.closeAuthDialog();

        var authDialog = document.createElement('div');
        authDialog.className = 'dialog-ux';
        authDialog.id = 'nexusstack-auth-dialog';

        authDialog.innerHTML = `<div class="backdrop-ux"></div>
        <div class="modal-ux">
            <div class="modal-dialog-ux">
                <div class="modal-ux-inner">
                    <div class="modal-ux-header">
                        <h3>接口授权登录</h3>
                        <button type="button" class="close-modal">
                            <svg width="20" height="20">
                                <use href="#close" xlink:href="#close"></use>
                            </svg>
                        </button>
                    </div>
                    <div class="modal-ux-content">
                        <div class="auth-form-wrapper"></div>
                        <div class="auth-btn-wrapper">
                            <button class="btn modal-btn auth btn-done button">取消</button>
                            <button type="submit" class="btn modal-btn auth authorize button">登录</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>`;

        var formWrapper = authDialog.querySelector('.auth-form-wrapper');

        // username
        createInput(formWrapper, 'username', '账号');

        // password
        createInput(formWrapper, 'password', '密码', 'password');

        document.getElementsByClassName('swagger-ui')[1].appendChild(authDialog);

        authDialog.querySelector('.btn-done.modal-btn').onclick = function () {
            nexusstack.swagger.closeAuthDialog();
        };

        authDialog.querySelector('.authorize.modal-btn').onclick = function () {
            nexusstack.swagger.login(loginCallback);
        };

        window.addEventListener("keydown", function (event) {
            console.log(event.key, "event.key");
            if (event.key === 'Enter') {
                nexusstack.swagger.login(loginCallback);
            }
        });
        authDialog.querySelector('.close-modal').onclick = function () {
            nexusstack.swagger.closeAuthDialog();
        };
    };

    nexusstack.swagger.closeAuthDialog = function () {
        if (document.getElementById('nexusstack-auth-dialog')) {
            document.getElementsByClassName('swagger-ui')[1].removeChild(document.getElementById('nexusstack-auth-dialog'));
        }
    };

    nexusstack.swagger.login = async function (callback) {
        var data = {
            userName: document.getElementById('username').value,
            // 密码暂时前端通过base64进行转码加密，swagger这里做特殊处理
            password: "swagger" + document.getElementById('password').value,
            platform: 0,
        };

        if (data.userName === "" || data.password === "") {
            alert("账号或密码不能为空");
            return;
        }

        await fetch(`${nexusstack.host}/api/basic/token/password`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        })
            .then((response) => response.json())
            .then(async (response) => {
                if (response.code === 200) {
                    var expireDate = new Date(response.data.expirationDate);
                    nexusstack.auth.setToken(response.data.token, expireDate);
                    callback();
                } else {
                    alert(response.message);
                }
            });
    };

    nexusstack.swagger.logout = function () {
        nexusstack.auth.clearToken();
    };

    function createInput(container, id, title, type) {
        var wrapper = document.createElement('div');
        wrapper.className = 'form-item';

        var label = document.createElement('label');
        label.innerText = title;
        label.className = 'form-item-label';
        wrapper.appendChild(label);

        var section = document.createElement('section');
        section.className = 'form-item-control';
        wrapper.appendChild(section);

        var input = document.createElement('input');
        input.id = id;
        input.type = type ? type : 'text';
        input.style.width = '100%';
        section.appendChild(input);

        container.appendChild(wrapper);
    }

    // 拦截 SwaggerUIBundle 的赋值，在配置初始化时注入自定义 authorizeBtn 插件。
    // nexusstack-swagger.js 通过 HeadContent 注入在 <head>，此时 <body> 里的
    // swagger-ui-bundle.js 尚未加载，因此可以通过 defineProperty setter 安全拦截。
    (function () {
        var _original;
        Object.defineProperty(window, 'SwaggerUIBundle', {
            configurable: true,
            get: function () { return _original; },
            set: function (fn) {
                _original = function (config) {
                    function getCssClass() {
                        return (nexusstack.auth && nexusstack.auth.getToken && nexusstack.auth.getToken()) ? 'cancel' : 'authorize';
                    }
                    function getText() {
                        return (nexusstack.auth && nexusstack.auth.getToken && nexusstack.auth.getToken()) ? '退出' : '登录';
                    }

                    config.plugins = (config.plugins || []).concat([function (system) {
                        return {
                            components: {
                                authorizeBtn: function () {
                                    return system.React.createElement(
                                        'div', { className: 'auth-wrapper' },
                                        system.React.createElement('button', {
                                            id: 'authorize',
                                            className: 'btn ' + getCssClass(),
                                            style: { lineHeight: 'normal' },
                                            onClick: function () {
                                                if (nexusstack.auth.getToken()) {
                                                    nexusstack.swagger.logout();
                                                    location.reload();
                                                } else {
                                                    nexusstack.swagger.openAuthDialog(function () { location.reload(); });
                                                }
                                            }
                                        }, getText())
                                    );
                                }
                            }
                        };
                    }]);
                    return fn(config);
                };
                Object.assign(_original, fn);
            }
        });
    })();
})();
