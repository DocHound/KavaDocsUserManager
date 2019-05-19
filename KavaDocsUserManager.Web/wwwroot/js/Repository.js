/// <reference path="../lib/jquery/dist/jquery.js" />
/// <reference path="../lib/ww.jquery.js" />
var vm = {};
var app = {};

vm = {
    repository: {},
    roles: [],
    roleUsers: [],
    allUsers: [],

    awesomplete: null,
    newUser: {
        username: "",
        userType: "contributor",
        visible: false,
        users: [],
        
        // search list related operations
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
        getName: function (item) {
            return item.name;
        }
    },
    newRole: {
        name: "",
        visible: false
    },
    initialize: function () {
        vm.repository = globals.repositoryWithUsers.repository;
        vm.users = globals.repositoryWithUsers.users;
        vm.allUsers = globals.repositoryWithUsers.allUsers;
        vm.roles = globals.repositoryWithUsers.roles;

        $("#Repository_Prefix").focus();

        setTimeout(function () {
            $('.selectpicker')
                .selectpicker()
                .on("changed.bs.select", vm.updateRoleForUser);
        }, 300);
        
        toastr.options.closeButton = true;
        toastr.options.positionClass = "toast-bottom-right";
    },

    // checkbox click in drop-down
    updateRoleForUser: function (e, clickedIndex, isSelected, previousValue) {
        var uid = e.target.dataset["userId"];

        var idx = vm.users.findIndex(function (u) {
            return u.userId == uid;
        });
        if (idx < 0)
            return;

        var user = vm.users[idx];
        var role = user.roles[clickedIndex];
        role.selected = isSelected;
        var url = "/api/repositories/" +
            vm.repository.id +
            "/updaterole/" +
            uid +
            "/" +
            role.roleId +
            "/" +
            isSelected;
        
        ajaxJson(url, null,
            function (success) {
                toastr.success("Role " + role.rolename + (isSelected ? " added to " : " removed from ") + user.username + ".", "Role updated");
            },
            function (error) {
                debugger;
                toastr.error("Failed to update role.");
            });
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
    addUserToRepo: function (repository, username, userType) {        
        ajaxJson("/api/repositories/" + repository.id + "/add/" + username + "/" + userType, null,
            function (repoUser) {                
                users.push(repoUser);
                vm.newUser.visible = false;
                vm.newUser.username = null;
                toastr.success(username + " has been added to this repository");
            },
            function(error) {
               toastr.error(error.message,"Couldn't add user");
            });
    },
    removeUserFromRepo: function (user) {
        ajaxJson("/api/repositories/" + vm.repository.id + "/remove/" + user.userId,
                null, 
                function () {
                    vm.users = vm.users.filter(function(u) {
                        return u.userId !== user.userId;
                    });
                    
                    toastr.success(user.username + " has been removed from the repository.");
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
    getRoles: function() {
        ajaxJson("/api/repositories/" + vm.repository.id + "/roles",
            null,
            function(roles) {  
                vm.roles = roles;
            },
            function(error) {
                toastr.error(error.message,"Couldn't retrieve roles");
            });
    },
    // reload the entire model from the server
    getUserRoles: function() {
        ajaxJson("/api/repositories/" + vm.repository.id + "/userroles",
            null,
            function(response) {
                vm.repository = response.repository;
                vm.roles = response.roles;

                // we have to explicitly rebuild the nested items
                vm.users.length = 0;
                for (var i = 0; i < response.users.length; i++) {
                    var user = response.users[i];
                    vm.users.push(user);

                    // this is the important one - otherwise roles doesn't refresh
                    Vue.set(user, "roles", user.roles);
                }
                

                // make sure the select picker gets updated otherwise values aren't refreshed.
                setTimeout(function() { $('.selectpicker').selectpicker("refresh"); }, 200);
            },
            function (error) {
                toastr.error(error.message, "Couldn't retrieve user roles");
            });
    },
    removeRole: function (role) {
        ajaxJson("/api/repositories/" + vm.repository.id + "/userroles/" + role.id,
            null,
            function (success) {

                vm.getUserRoles();

                toastr.success("Role " + role.name + " removed.");
            },
            function (error) {
                toastr.error(error.message, "Couldn't delete role");
            },
            { method: "DELETE" });
    },
    addRole: function () {

        var roleName = vm.newRole.name;
        if (!roleName)
            return;

        ajaxJson("/api/repositories/" + vm.repository.id + "/role/add",
            { roleName: roleName },
            function (role) {
                vm.newRole.name = "";
                vm.newRole.visible = false;

                vm.getUserRoles();

                toastr.success("Role " + roleName + " added.");
            },
            function (error) {
                toastr.error(error.message, "Couldn't delete role");
            },
            {method: "POST"});
    },
    navigateDomain: function (e) {
        var url = "https://" + this.$refs.domainPrefix.value + "." + this.$refs.domainName.value;
        window.open(url);
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
};

vm.initialize();

app = new Vue({
    el: "#RepositoryPage",
    data: vm
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

