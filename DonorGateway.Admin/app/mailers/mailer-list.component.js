//mailer-list.component.js
(function () {
    var module = angular.module('app');

    function controller($http, toastr, $modal) {
        var $ctrl = this;
        var tableStateRef;
        var pageSizeDefault = 10;

        $ctrl.title = 'MarkIt Manager';
        $ctrl.subTitle = 'Mailers';

        $ctrl.searchModel = {
            page: 1,
            pageSize: pageSizeDefault,
            orderBy: 'campaignId',
            orderDirection: 'desc',
            suppress: false
        };

        $ctrl.$onInit = function () {
            console.log('mail list init');
            $http.get('api/mailer/campaigns').then(function (r) {
                $ctrl.campaigns = r.data;
            });
            $http.get('api/mailer/reasons').then(function (r) {
                $ctrl.reasons = r.data;
            });
        }

        $ctrl.download = function () {
            $ctrl.isBusy = true;
            $http.post('api/mailer/export', $ctrl.searchModel)
                .then(function (data) {
                    var contentType = data.headers()['content-type'];
                    var filename = data.headers()['x-filename'];

                    var linkElement = document.createElement('a');
                    try {
                        var blob = new Blob([data.data], { type: contentType });
                        var url = window.URL.createObjectURL(blob);

                        linkElement.setAttribute('href', url);
                        linkElement.setAttribute("download", filename);

                        var clickEvent = new MouseEvent("click", {
                            "view": window,
                            "bubbles": true,
                            "cancelable": false
                        });
                        linkElement.dispatchEvent(clickEvent);
                    } catch (ex) {
                        console.log(ex);
                        toastr.error('Oops ' + ex.data.message);
                    }
                }).finally(function () {
                    $ctrl.isBusy = false;
                });
        }

        $ctrl.search = function (tableState) {
            tableStateRef = tableState;
            $ctrl.isBusy = true;

            if (typeof (tableState.sort.predicate) !== "undefined") {
                $ctrl.searchModel.orderBy = tableState.sort.predicate;
                $ctrl.searchModel.orderDirection = tableState.sort.reverse ? 'desc' : 'asc';
            }

            if ($ctrl.searchModel.suppress === null) $ctrl.searchModel.suppress = false;
            return $http.get('api/mailer', { params: $ctrl.searchModel })
                .then(function (r) {
                    $ctrl.mailers = r.data.results;
                    $ctrl.searchModel = r.data;
                    delete $ctrl.searchModel.results;
                }).catch(function (err) {
                    console.log('Oops', err.data.message);
                    toastr.error('Oops ' + err.data.message);
                }).finally(function () {
                    $ctrl.isBusy = false;
                });
        }

        $ctrl.showUpload = function () {
            $modal.open({
                component: 'mailerUpload',
                bindings: {
                    modalInstance: "<"
                },
                size: 'md'
            }).result.then(function (result) {
                toastr.info('Uploaded ' + result.title);
            }, function (reason) {
            });
        }

        $ctrl.paged = function paged() {
            $ctrl.search(tableStateRef);
        };

        $ctrl.toggleFilter = function () {
            $ctrl.showSuppress = !$ctrl.showSuppress;
            $ctrl.searchModel.suppress = $ctrl.showSuppress;
            $ctrl.search(tableStateRef);
        }

        $ctrl.toggleSuppress = function (mailer) {
            if (mailer.suppress) {
                mailer.suppress = false;
                mailer.reasonId = null;
                return $http.put('api/mailer', mailer).then(function (r) {
                    angular.extend(mailer, r.data);
                }).catch(function (err) {
                    console.log('Error saving mailer', err.message);
                    $toastr.error('Oops ' + err.data.message);
                }).finally(function () {
                });
            } else {
                $modal.open({
                    component: 'mailerSuppress',
                    bindings: {
                        modalInstance: "<"
                    },
                    resolve: {
                        mailer: mailer
                    },
                    size: 'md'
                }).result.then(function (result) {
                    angular.extend(mailer, result);
                    toastr.info('Saved ' + result.firstName + ' ' + result.lastName);
                }, function (reason) {
                });
            }
        }
    }

    module.component('mailerList',
        {
            templateUrl: 'app/mailers/mailer-list.component.html',
            controller: ['$http', 'toastr', '$uibModal', controller]
        });
}
)();