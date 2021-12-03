if (!ITVenture.Pages.TaskScheduler) {
    ITVenture.Pages.TaskScheduler = {};
}
ITVenture.Pages.TaskScheduler.ServicePlugins = {
    reorderer: null,
    rebind: function(e) {
        console.log(e);
        if (e.type === "create" || e.type === "update") {
            e.sender.read();
        }
        //e.sender.dataSource.read();
        //e.sender.refresh();
    },
    error_handler: function(e) {
        var rollback_grid = false;
        if (e.errors) {
            var message = "Errors:\n";
            $.each(e.errors, function (key, value) {
                rollback_grid = key == "Error" ? true : false;
                if ('errors' in value) {
                    $.each(value.errors, function () {
                        message += this + "\r\n";
                    });
                }
            });
            var notificationWidget = $("#notification").data("kendoNotification");
            if (rollback_grid) {
                notificationWidget.error(message);
                this.cancelChanges();
            }
            else {
                notificationWidget.info(message);
                this.cancelChanges();
                this.read();
            }
        }
    },
    renderAssemblyInfo: function(e) {
        ITVenture.Tools.AssemblyAnalyzerDataSource.InitializeFor($('#PluginDetailInfo'));
        $.ajax({
            url: ITVenture.Helpers.ResolveUrl("~/ScheduledTasks/ServicePlugins/Details?AssemblyName=" + e),
            success: function(data) {
                //console.log(data);
                $('#PluginDetailInfo').html(data);
            }
        });
    },
    initReordering: function(e) {
        var currentName = e.sender.element.attr("id");
        if (ITVenture.Pages.TaskScheduler.ServicePlugins.reorderer == null) {
            ITVenture.Pages.TaskScheduler.ServicePlugins.reorderer = ITVenture.Tools.RowReordering.Initialize(currentName, function (entity, newIndex) {
                entity.LoadOrder = newIndex;
            },true);
        }
        ITVenture.Pages.TaskScheduler.ServicePlugins.reorderer.styleDraggers();
    },
    toggleReordering: function(e) {
        var reorderControl = ITVenture.Pages.TaskScheduler.ServicePlugins.reorderer;
        reorderControl.enabled = !reorderControl.enabled;
        $(e.currentTarget).text(reorderControl.enabled ? "Disable Reorder" : "EnableReorder");
    }
};
$(document).ready(function () {
    //exapand
    $(".panel-wrap").on("click", "span.k-i-arrow-chevron-down", function (e) {
        var contentElement = $(e.target).closest(".widgetbox").find(">div");
        $(e.target)
            .removeClass("k-i-arrow-chevron-down")
            .addClass("k-i-arrow-chevron-up");

        kendo.fx(contentElement).expand("vertical").stop().play();
    });

    //collapse
    $(".panel-wrap").on("click", "span.k-i-arrow-chevron-up", function (e) {
        var contentElement = $(e.target).closest(".widgetbox").find(">div");
        $(e.target)
            .removeClass("k-i-arrow-chevron-up")
            .addClass("k-i-arrow-chevron-down");

        kendo.fx(contentElement).expand("vertical").stop().reverse();
    });
});