//upload-status.component.js
(function () {
    var module = angular.module('app');

    function controller($http, log) {
        var $ctrl = this;
        $ctrl.title = 'Upload History';
        $ctrl.isBusy = false; 

        $ctrl.$onInit = function () {
            console.log('Upload Status Init');
            $ctrl.refresh();
        }

        $ctrl.cancel = function () {
            $ctrl.dismiss();
        }

        $ctrl.refresh = function () {
            console.log('refresh history'); 
            $ctrl.isBusy = true; 
            $http.get('/api/acquisitions').then(function (r) {
                $ctrl.history = r.data;
            }).catch(function() {
                log.error('Error retrieving history'); 
            }).finally(function() {
                $ctrl.isBusy = false; 
            });
        }

    }

    module.component('uploadHistory',
        {
            bindings: {
                id: '<',
                resolve: '<',
                close: '&',
                dismiss: '&',
                modalInstance: '<'
            },
            templateUrl: 'app/mailers/upload-history.component.html',
            controller: ['$http', 'toastr', controller]
        });

}
)();