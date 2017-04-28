//mailer-upload.component.js
(function () {
    var module = angular.module('app');

    function formDataObject(data) {
        var fd = new FormData();
        fd.append('file', data);
        return fd;
    }

    function controller($http, toastr) {
        var $ctrl = this;
        $ctrl.title = 'Mailer Upload';

        $ctrl.$onInit = function () {
            console.log('Mailer Upload Init');
            $http.get('api/mailer/campaigns').then(function (r) {
                $ctrl.campaigns = r.data;
            });
        }

        $ctrl.cancel = function () {
            $ctrl.file = undefined;
            $ctrl.dismiss();
        }

        $ctrl.fileSelected = function ($file, $event) {
            $ctrl.result = null;
        };

        $ctrl.saveCampaign = function () {
            $http.post('api/mailer/createcampaign', $ctrl.campaign).then(function (r) {
                $ctrl.campaigns.unshift(r.data);
                $ctrl.selectedCampaign = r.data;
                $ctrl.showCreate = false;
            }).catch(function (err) {
                toastr.error('Oops ' + err.data.message);
            });
        }

        $ctrl.upload = function () {
            $ctrl.isBusy = true;
            $ctrl.result = {
                success: false
            }
            var id = $ctrl.selectedCampaign.id; 

            return $http.post('api/file/mailer/' + id, formDataObject($ctrl.file), {
                transformRequest: angular.identity,
                headers: { 'Content-Type': undefined }
            }).then(function (r) {
                $ctrl.result.success = true;
                $ctrl.result.message = r.data.message[0];
            }).catch(function (error) {
                $ctrl.result.message = error.data.message;
            }).finally(function () {
                $ctrl.file = undefined;
                $ctrl.isBusy = false;
                $ctrl.result.campaigns = $ctrl.campaigns;
                $ctrl.modalInstance.close($ctrl.result);
            });

            //service.mailer(vm.selectedCampaign.id, vm.file)
            //    .then(function (data) {
            //        $ctrl.result.success = true;
            //        $ctrl.result.message = data;
            //    })
            //    .catch(function (error) {
            //        $ctrl.result.message = error.data.message;
            //    })
            //    .finally(function () {
            //        $ctrl.file = undefined;
            //        $ctrl.isBusy = false;
            //        $ctrl.result.campaigns = $ctrl.campaigns;
            //        $ctrl.modalInstance.close($ctrl.result);
            //    });
        }
    }

    module.component('mailerUpload',
        {
            bindings: {
                id: '<',
                resolve: '<',
                close: '&',
                dismiss: '&',
                modalInstance: '<'
            },
            templateUrl: 'app/mailers/mailer-upload.component.html',
            controller: ['$http', 'toastr', controller]
        });

}
)();