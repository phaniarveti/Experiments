/**
 * @ngdoc controller
 * @name Umbraco.Editors.Media.EditController
 * @function
 * 
 * @description
 * The controller for the media editor
 */
function mediaEditController($scope, $routeParams, appState, mediaResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, fileManager, treeService, formHelper, umbModelMapper, editorState) {

    //setup scope vars
    $scope.nav = navigationService;
    $scope.currentSection = appState.getSectionState("currentSection");
    $scope.currentNode = null; //the editors affiliated node

    if ($routeParams.create) {

        mediaResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function (data) {
                $scope.loaded = true;
                $scope.content = data;

                editorState.set($scope.content);
            });
    }
    else {
        mediaResource.getById($routeParams.id)
            .then(function (data) {
                $scope.loaded = true;
                $scope.content = data;
                
                editorState.set($scope.content);

                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();

                navigationService.syncTree({ tree: "media", path: data.path }).then(function (syncArgs) {
                    $scope.currentNode = syncArgs.node;
                });
                
            });
    }
    
    $scope.save = function () {

        if (!$scope.busy && formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

            $scope.busy = true;

            mediaResource.save($scope.content, $routeParams.create, fileManager.getFiles())
                .then(function(data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    editorState.set($scope.content);
                    $scope.busy = false;

                    navigationService.syncTree({ tree: "media", path: data.path, forceReload: true }).then(function (syncArgs) {
                        $scope.currentNode = syncArgs.node;
                    });

                }, function(err) {

                    contentEditingHelper.handleSaveError({
                        err: err,
                        redirectOnFailure: true,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, err.data)
                    });
                    
                    editorState.set($scope.content);
                    $scope.busy = false;
                });
        }else{
            $scope.busy = false;
        }
        
    };
}

angular.module("umbraco")
    .controller("Umbraco.Editors.Media.EditController", mediaEditController);