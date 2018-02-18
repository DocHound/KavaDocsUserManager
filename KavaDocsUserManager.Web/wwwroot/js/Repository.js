/// <reference path="../lib/jquery/dist/jquery.js" />
/// <reference path="../lib/ww.jquery.js" />
var vm = {}


vm = {
    repository: {},
    newUser: {
        awesomplete: null,
        username: "",        
        type: 'contributor',
        visible: false,
        userList: [],        
        getName: function(item) {
            return item.name;
        },
        getUserSearchList: function (event) {
            if (event.key == "ArrowDown" || event.Key == "ArrowUp" || event.Key == "Enter")
                return;
            
            if (!vm.newUser.username || vm.newUser.username.length < 2)
                return;
            
            ajaxJson("/api/repositories/searchusers/" + vm.newUser.username,
                null,
                function (list) {
                    console.log(list);
                    vm.newUser.userList = list;
                    vm.newUser.awesomplete.list = list;
                },
                function () {

                }, { method: "GET" });
        },
    },
    initialize: function() {
        vm.repository = globals.repository;
        $("#Repository_Prefix").focus();

        toastr.options.closeButton = true;
        toastr.options.positionClass = "toast-bottom-right";        
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
    optionalFieldSelection: function (event) {        
        var el$ = $(event.currentTarget);
        var value = el$.val();
        $(".optional-field-group textarea.code-editing").addClass("hidden");
        var item$ = $("#" + value);
        console.log(item$,value);
        item$.removeClass("hidden");
    },
    addUserToRepo: function (repository, username) {        
        ajaxJson("/api/repositories/" + repository.id + "/add/" + username, null,
            function (repoUser) {                
                repository.users.push(repoUser);
                vm.newUser.visible = false;
                vm.newUser.username = null;
                toastr.success(username + " has been added to this repository");
            },
            function(error) {
               toastr.error(error.message,"Couldn't add user");
            });
    },
    removeUserFromRepo: function(user, repository) {
        ajaxJson("/api/repositories/" + repository.id + "/remove/" + user.id,
                null,
                function () {
                    repository.users = repository.users.filter(function(u) {
                        return u.user !== user;
                    });
                    
                    toastr.success(user.userDisplayName + " has been removed from the repository.");
                },
                function(error) {
                    toastr.error(error.message, "Couldn't remove user");
                });                
    },
    deleteRepository: function() {
        if (!confirm("Are you sure you want to delete this repository?"))
            return; 

        ajaxJson("/api/repositories/" + vm.repository.id,
            null,
            function(result) {
                window.location = "/repositories";
            },
            function(error) {
                vm.status(error.message);
            },
            { method: "DELETE" });
    },
    // info, success, warning*
    status: function (message, icon, title) {
        if (!message)
            return;

        if (!icon)
            icon = "warning";

        if(this.icon == "info")
            toastr.info(this.message, title);
        else if (this.icon == "success")
            toastr.success(this.message,title);
        else
            toastr.warning(this.message, title);
    }
}

vm.initialize();
var app = new Vue({
    el: "#RepositoryPage",
    data: function() {
         return vm;
    }
});
//Vue.use(VAutocomplete.default);

setTimeout(function () {
    var el = document.getElementById("UserToAdd");
    vm.newUser.awesomplete =
        new Awesomplete(el,
            {
                list: vm.newUser.userList,
                selectcomplete: function(item) {                    
                    vm.newUser.searchname = item.text;
                }
            });
    el.addEventListener("awesomplete-selectcomplete",
        function(event) {
            var item = event.text;  // selected 'item'
            vm.newUser.username = item.value; 
        });
}, 2000);

