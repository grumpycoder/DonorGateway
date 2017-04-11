//mailer-suppress.component.js
(function () {
    var module = angular.module('app');

    function controller($http) {
        var $ctrl = this;

        $ctrl.$onInit = function () {
            console.log('mailer suppress init');
            if ($ctrl.resolve) {
                $ctrl.mailer = $ctrl.resolve.mailer;
                $ctrl.title = $ctrl.mailer.firstName + ' ' + $ctrl.mailer.lastName;
            }

            $http.get('api/mailer/reasons').then(function (r) {
                $ctrl.reasons = r.data;
            });
        }

        $ctrl.cancel = function() {
            $ctrl.dismiss();    
        }

        $ctrl.save = function () {
            $ctrl.mailer.suppress = true; 
            return $http.put('api/mailer', $ctrl.mailer).then(function (r) {
                angular.extend($ctrl.mailer, r.data);
                $ctrl.modalInstance.close($ctrl.mailer);
            }).catch(function (err) {
                console.log('Error saving mailer', err.message);
            }).finally(function () {
            });
        }
    };

    module.component('mailerSuppress',
        {
            templateUrl: 'app/mailers/mailer-suppress.component.html',
            bindings: {
                id: '<',
                resolve: '<',
                close: '&',
                dismiss: '&',
                modalInstance: '<'
            },
            controller: ['$http', controller]
        });
}
)();