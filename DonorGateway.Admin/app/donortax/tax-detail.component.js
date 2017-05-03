//tax-detail.component.js
(function () {
    var module = angular.module('app');

    function getYears(currentYear) {
        var years = [];
        //var currentYear = parseInt(moment().get('Year'));

        for (var i = 0; i < 5; i++) {
            years.push(currentYear - i);
        }
        return years;
    }

    function controller($http, toastr) {
        var $ctrl = this;
        var currentYear = parseInt(moment().get('Year'));

        $ctrl.$onInit = function () {
            console.log('tax detail init');
            $ctrl.isBusy = true;
            $ctrl.newItem = {};

            if ($ctrl.resolve) {
                $ctrl.person = $ctrl.resolve.person;
            }

            $ctrl.title = $ctrl.person.firstName + ' ' + $ctrl.person.lastName;
            $ctrl.selectedYear = currentYear - 1;
            $ctrl.years = getYears(currentYear);
            $ctrl.yearChange();

            $http.get('api/tax/' + $ctrl.person.lookupId).then(function (r) {
                $ctrl.taxItems = r.data;
            }).finally(function () {
                $ctrl.isBusy = false;
            });
        }

        $ctrl.addItem = function() {
            $ctrl.isBusy = true;

            $ctrl.newItem.constituentId = $ctrl.person.id;
            $ctrl.newItem.taxYear = moment($ctrl.newItem.donationDate).year();

            $http.post('api/tax', $ctrl.newItem ).then(function(r) {
                $ctrl.taxItems.push(r.data);
                toastr.success('Saved tax entry');
            }).catch(function(err) {
                toastr.error('Oops. Error creating tax entry');
            }).finally(function() {
                $ctrl.isBusy = false; 
            });
        }

        $ctrl.cancel = function () {
            $ctrl.dismiss();
        }

        $ctrl.cancelEdit = function () {
            $ctrl.currentEdit = {};
        }

        $ctrl.delete = function(item) {
            $ctrl.isBusy = true;

            $http.delete('api/tax/' + item.id).then(function(r) {
                toastr.warning('Delete tax item ' + item.amount);
                var idx = $ctrl.taxItems.indexOf(item);
                $ctrl.taxItems.splice(idx, 1);
            }).catch(function(err) {
                toastr.error('Oops. Error deleting tax');
            }).finally(function() {
                $ctrl.isBusy = false;
            }); 
        }

        $ctrl.edit = function (item) {
            console.log('item', item);
            $ctrl.currentEdit = {};
            $ctrl.currentEdit[item.id] = true;
            $ctrl.itemToEdit = angular.copy(item);
            $ctrl.itemToEdit.donationDate = moment($ctrl.itemToEdit.donationDate).toDate();
        }

        $ctrl.save = function (item) {
            $ctrl.isBusy = true;
            $http.put('api/tax', $ctrl.itemToEdit)
                .then(function (r) {
                    angular.extend(item, r.data);
                    $ctrl.currentEdit = {};
                    toastr.success('Updated tax item ' + moment(item.donationDate).format('MM/dd/yyyy'));
                }).catch(function(err) {
                    toastr.error('Error saving tax entry');
                }).finally(function() {
                    $ctrl.isBusy = false; 
                });
        }

        $ctrl.yearChange = function () {
            $ctrl.dateOptions = {
                maxDate: new Date('12/30/' + $ctrl.selectedYear),
                minDate: new Date('1/1/' + $ctrl.selectedYear)
            };
            $ctrl.newItem.donationDate = $ctrl.dateOptions.minDate;
        }

    }

    module.component('taxDetail',
        {
            bindings: {
                person: '<',
                resolve: '<',
                close: '&',
                dismiss: '&',
                modalInstance: '<'
            },
            templateUrl: 'app/donortax/tax-details.component.html',
            controller: ['$http', 'toastr', controller]
        });

}
)();