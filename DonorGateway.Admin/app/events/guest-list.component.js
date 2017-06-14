//guest-list.component.js
(function () {
    var module = angular.module('app');

    function guestListController($http, $modal, toastr) {
        var ctrl = this;
        var tableStateRef;
        var pageSizeDefault = 10;
        var choices =
            [
            { id: 1, name: "Register", command: function (e) { ctrl.registerGuest(e) }, icon: 'icon ion-key', default: true },
            { id: 2, name: "Mail Ticket", command: function (e) { ctrl.sendMail(e) }, icon: 'icon ion-android-mail', default: false },
            { id: 3, name: "Cancel", command: function (e) { ctrl.cancelRegistration(e) }, icon: 'icon ion-android-cancel', default: false },
            { id: 4, name: "Guest List", command: function (e) { ctrl.reservationOverride(e) }, icon: 'icon ion-android-add-circle', default: false },
            { id: 5, name: "Add Tickets", command: function (e) { ctrl.registerGuest(e) }, icon: 'icon ion-android-add-circle', default: false }
        ];
        
        ctrl.title = 'Reservation Manager';
        ctrl.description = "Manage Guest List";

        ctrl.searchModel = {
            page: 1,
            pageSize: pageSizeDefault,
            orderBy: 'id',
            orderDirection: 'asc'
        };

        ctrl.$onChanges = function () {
            ctrl.search();
        }

        ctrl.$onInit = function () {
            console.log('guest list init');
        }

        ctrl.resetSearch = function () {
            ctrl.searchModel = {
                page: 1,
                pageSize: pageSizeDefault,
                orderBy: 'id',
                orderDirection: 'asc'
            };
            ctrl.quickFilter = null;
            ctrl.search(tableStateRef);
        }

        ctrl.search = function () {
            ctrl.isBusy = true;
            if (ctrl.eventId === undefined) return;
            $http.get('api/event/' + ctrl.eventId + '/guests', { params: ctrl.searchModel }).then(function (r) {
                ctrl.searchModel = r.data;
                ctrl.guests = [];
                r.data.results.map(function (guest) {
                    guest.choices = buildGuestOptions(guest);
                    ctrl.guests.push(guest);
                });
                delete ctrl.searchModel.results;
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
            }).finally(function () {
                ctrl.isBusy = false;
            });
        }

        ctrl.paged = function paged() {
            ctrl.search(tableStateRef);
        };

        ctrl.quickFilterChange = function () {
            ctrl.searchModel.page = 1;
            ctrl.searchModel.isWaiting = null;
            ctrl.searchModel.isMailed = null;
            ctrl.searchModel.isAttending = null;

            ctrl.showSendMail = null; 
            ctrl.showSendWaiting = null; 

            switch (ctrl.quickFilter) {
                case 'WaitingAndNotSent':
                    ctrl.searchModel.isWaiting = true;
                    ctrl.searchModel.isMailed = false;
                    ctrl.showSendWaiting = true; 
                    break;
                case 'WaitingAndSent':
                    ctrl.searchModel.isWaiting = true;
                    ctrl.searchModel.isMailed = true;;
                    break;
                case 'TicketNotSent':
                    ctrl.searchModel.isAttending = true;
                    ctrl.searchModel.isMailed = false;
                    ctrl.searchModel.isWaiting = false;
                    ctrl.showSendMail = true; 
                    break;
                case 'TicketSent':
                    ctrl.searchModel.isAttending = true;
                    ctrl.searchModel.isMailed = true;
                    ctrl.searchModel.isWaiting = false;
                    break;
                default:
            }
            ctrl.search(tableStateRef);
        }

        ctrl.reservationOverride = function (e) {
            $http.post('api/event/' + ctrl.eventId + '/register/', e).then(function (r) {
                var guest = r.data;
                guest.choices = buildGuestOptions(guest);
                angular.extend(e, guest);
                toastr.success('Registered ' + guest.name);
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
                toastr.error('Oops. Something went wrong registering guest', err.data.message);
            });
        }

        ctrl.cancelRegistration = function (e) {
            $http.post('api/event/' + ctrl.eventId + '/cancelregister/' + e.id).then(function (r) {
                var guest = r.data;
                guest.choices = buildGuestOptions(guest);
                angular.extend(e, guest);
                toastr.success('Canceled registration for ' + guest.name);
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
                toastr.error('Oops. Something went wrong cancelling registration', err.data.message);
            });
        }

        ctrl.registerGuest = function (e) {
            var newGuest = false;
            if (!e) {
                e = { id: null, eventId: ctrl.eventId }
                newGuest = true;
            }

            $modal.open({
                component: 'guestEdit',
                bindings: {
                    modalInstance: "<"
                },
                resolve: {
                    guestId: e.id,
                    eventId: e.eventId
                },
                size: 'md'
            }).result.then(function (result) {
                result.choices = buildGuestOptions(result);
                angular.extend(e, result);
                if (newGuest) ctrl.guests.unshift(e);
                toastr.info('Registered ' + result.name);
            }, function (reason) {
            });
        }

        ctrl.sendMail = function (e) {
            $http.post('api/event/' + ctrl.eventId + '/mailticket/' + e.id).then(function (r) {
                var guest = r.data;
                guest.choices = buildGuestOptions(guest);
                angular.extend(e, guest);
                toastr.success('Mailed ticket for ' + guest.name);
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
                toastr.error('Oops. Something went wrong mailing ticket', err.data.message);
            });
        }

        ctrl.sendAllMail = function() {
            $http.post('api/event/' + ctrl.eventId + '/sendalltickets').then(function (r) {
                toastr.success('Mailed all tickets');
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
                toastr.error('Oops. Something went wrong mailing tickets', err.data.message);
            });
        }

        ctrl.sendAllWaiting = function () {
            $http.post('api/event/' + ctrl.eventId + '/sendallwaiting').then(function (r) {
                toastr.success('Mailed all letters');
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
                toastr.error('Oops. Something went wrong mailing letters', err.data.message);
            });
        }

        ctrl.export = function() {
            ctrl.isBusy = true;
            $http.get('api/event/' + ctrl.eventId + '/guests/export', { params: ctrl.searchModel })
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
                        logger.log(ex);
                    }
                }).finally(function () {
                    ctrl.isBusy = false;
                });
        }

        ctrl.import = function() {
            $modal.open({
                component: 'guestImport',
                bindings: {
                    modalInstance: "<"
                },
                resolve: {
                    eventId: ctrl.eventId
                },
                size: 'md'
            }).result.then(function (result) {
                ctrl.search(tableStateRef);
            }, function (reason) {
            });
        }

        function buildGuestOptions(guest) {
            var options = [];

            if (guest.canRegister) options.push(choices[0]);
            if (guest.canMail) options.push(choices[1]);
            if (guest.canAddToAttending) options.push(choices[3]);

            if (guest.canAddTickets) options.push(choices[4]);
            if (guest.canCancel) options.push(choices[2]);

            guest.primaryChoice = angular.copy(options[0]);
            options.shift();
            return options;
        }

    }

    module.component('guestList',
        {
            bindings: {
                eventId: '<'
            },
            templateUrl: 'app/events/guest-list.component.html',
            controller: ['$http', '$uibModal', 'toastr', guestListController]
        });

}
)();