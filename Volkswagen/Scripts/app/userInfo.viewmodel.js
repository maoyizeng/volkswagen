function UserInfoViewModel(app, name, dataModel) {
    var self = this;

    // 数据
    self.name = ko.observable(name);

    // 操作
    self.logOff = function () {
        dataModel.logout().done(function () {
            app.navigateToLoggedOff();
        }).fail(function () {
            app.errors.push("注销失败。");
        });
    };

    self.manage = function () {
        app.navigateToManage();
    };
}
