//constituent-edit.component.js
(function () {
    var module = angular.module('app');

    function controller($http) {
        var $ctrl = this;

        $ctrl.$onInit = function () {
            console.log('constituent edit init');
            if ($ctrl.resolve) {
                $ctrl.person = angular.copy($ctrl.resolve.person);
            }

            $ctrl.title = $ctrl.person.name;
        }

        $ctrl.cancel = function () {
            $ctrl.dismiss();
        }

        $ctrl.save = function () {
            console.log('save', $ctrl.person);
            $http.put('api/constituent', $ctrl.person).then(function(r) {
                $ctrl.modalInstance.close($ctrl.person);
            });
        }
    }

    module.component('constituentEdit',
        {
            bindings: {
                person: '<',
                resolve: '<',
                close: '&',
                dismiss: '&',
                modalInstance: '<'
            },
            templateUrl: 'app/donortax/constituent-edit.component.html',
            controller: ['$http', controller]
        });

}
)();