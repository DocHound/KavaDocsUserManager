/// <reference path="../lib/jquery/dist/jquery.js" />
/// <reference path="../lib/ww.jquery.js" />
var vm = {}

vm = {
    repository: {},
    newUser: {
        username: "Phantomias",
        type: 'contributor',
        visible: false
    },
    initialize: function() {
        vm.repository = globals.repository;

    },
    highlightCode: function() {
        $("pre code")
            .each(function (i, block) {
                hljs.highlightBlock(block);
            });        
    },
    editCodeField: function(elPreview, elCode) {
        var preview$ = $("#" + elPreview);
        var code$ = $("#" + elCode);
        code$.show();
        code$.height(preview$.height());
        preview$.hide();

    },
    hideCodeField: function(elPreview, elCode) {
        var preview$ = $("#" + elPreview);
        var code$ = $("#" + elCode);

        code$.hide();
        preview$.show();
        preview$.html(code$.val());
        setTimeout(vm.highlightCode,10);
    },
    addUserToRepo: function (repository, username) {        
        ajaxJson("/api/repositories/" + repository.id + "/add/" + username, null,
            function (repoUser) {
                debugger;
                repository.users.push(repoUser);
                vm.newUser.visible = false;
                vm.newUser.username = null;
            },
            function(error) {
               vm.status("Unable to add user");
            });
    },
    removeUserFromRepo: function(user, repository) {
        ajaxJson("/api/repositories/" + repository.id + "/remove/" + user.id,
                null)
            .then(function () {                    
                    repository.users = repository.users.filter(function(u) {
                        return  u.user !== user;
                    });                    
                },
                function(error) {
                    vm.status("Updated failed: " + error.message);
                });                
    },
    status: function (message) {
        if (message)
            alert(message);
    }

}
vm.initialize();

var app = new Vue({
    el: "#RepositoryPage",
    data: function() {
         return vm;
    }
});
