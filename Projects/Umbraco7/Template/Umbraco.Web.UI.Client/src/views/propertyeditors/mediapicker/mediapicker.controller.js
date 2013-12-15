//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.PropertyEditors.MediaPickerController",
    function($rootScope, $scope, dialogService, mediaResource, imageHelper, $log) {

        //check the pre-values for multi-picker
        var multiPicker = $scope.model.config.multiPicker !== '0' ? true : false;

        function setupViewModel() {
            $scope.images = [];
            $scope.ids = []; 

            if ($scope.model.value) {
                $scope.ids = $scope.model.value.split(',');

                mediaResource.getByIds($scope.ids).then(function (medias) {
                    //img.media = media;
                    _.each(medias, function (media, i) {
                        media.src = imageHelper.getImagePropertyValue({ imageModel: media });
                        media.thumbnail = imageHelper.getThumbnailFromPath(media.src);
                        $scope.images.push(media);
                    });
                });
            }
        }

        setupViewModel();

        $scope.remove = function(index) {
            $scope.images.splice(index, 1);
            $scope.ids.splice(index, 1);
            $scope.sync();
        };

        $scope.add = function() {
            dialogService.mediaPicker({
                multiPicker: multiPicker,
                callback: function(data) {
                    
                    //it's only a single selector, so make it into an array
                    if (!multiPicker) {
                        data = [data];
                    }
                    
                    _.each(data, function(media, i) {
                        media.src = imageHelper.getImagePropertyValue({ imageModel: media });
                        media.thumbnail = imageHelper.getThumbnailFromPath(media.src);
                        
                        $scope.images.push(media);
                        $scope.ids.push(media.id);
                    });

                    $scope.sync();
                }
            });
        };

       $scope.sortableOptions = {
           update: function(e, ui) {
                var r = [];
                angular.forEach($scope.renderModel, function(value, key){
                    r.push(value.id);
                });

                $scope.ids = r;
                $scope.sync();
            }
        };

        $scope.sync = function() {
            $scope.model.value = $scope.ids.join();
        };

        $scope.showAdd = function () {
            if (!multiPicker) {
                if ($scope.model.value && $scope.model.value !== "") {
                    return false;
                }
            }
            return true;
        };

        //here we declare a special method which will be called whenever the value has changed from the server
        //this is instead of doing a watch on the model.value = faster
        $scope.model.onValueChanged = function (newVal, oldVal) {
            //update the display val again if it has changed from the server
            setupViewModel();
        };

    });