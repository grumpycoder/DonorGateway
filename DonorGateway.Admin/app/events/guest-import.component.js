//guest-import.component.js
(function () {
    var module = angular.module('app');

    function formDataObject(data) {
        var fd = new FormData();
        fd.append('file', data);
        return fd;
    }

    function controller($http, log) {
        var $ctrl = this;

        $ctrl.title = 'Upload Guest List';

        $ctrl.$onInit = function () {
            console.log('guest import init');
            if ($ctrl.resolve) {
                $ctrl.eventId = $ctrl.resolve.eventId;
            }
        }

        $ctrl.fileSelected = function ($file, $event) {
            $ctrl.result = null;
        };

        $ctrl.cancel = function () {
            $modal.dismiss();
        }

        $ctrl.save = function () {
            $ctrl.isBusy = true; 
            $http.post('api/file/guest/' + $ctrl.eventId, formDataObject($ctrl.file), {
                transformRequest: angular.identity,
                headers: { 'Content-Type': undefined }
            }).then(function (r) {
                log.success(r.data.messages[0] + ' in ' + r.data.totalTime);
                console.log('response', r.data);
                $ctrl.isBusy = false; 
                $ctrl.modalInstance.close();
            }).catch(function(err) {
                console.log('Oops. Something when wrong', err);
            });
        }

    }

    module.component('guestImport',
        {
            bindings: {
                eventId: '<',
                resolve: '<',
                close: '&',
                dismiss: '&',
                modalInstance: '<'
            },
            templateUrl: 'app/events/guest-import.component.html',
            controller: ['$http', 'toastr', controller]
        });

}
)();