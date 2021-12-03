if (!ITVenture.Pages.TaskScheduler) {
    ITVenture.Pages.TaskScheduler = {};
}

ITVenture.Pages.TaskScheduler.TaskScheduler = {
    removeFromArray: function(arr, value) {
        for (var i = 0; i < arr.length; i++) {
            if (arr[i] === value) {
                delete arr[i];
                return;
            }
        }
    },
    addToArray: function(arr, value) {
        for (var i = 0; i < arr.length; i++) {
            if (arr[i] === value) return;
        }

        arr.push(value);
    },
    isInArr: function(arr, value) {
        for (var i = 0; i < arr.length; i++) {
            if (arr[i] === value) return true;
        }

        return false;
    },
    rebind: function(e) {
        console.log(e);
        if (e.type === "create" || e.type === "update") {
            e.sender.read();
        }
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
    expandedRows: [],
    Index: {
        AssignedSchedule: {
            Schedule: {
                updateWeekdays: function(e) {
                    var PeriodicScheduleId = e.sender.element.attr("PeriodicScheduleId");
                    var day = e.sender.element.attr("name");
                    var usc = day.indexOf("_");
                    day = day.substr(0, usc);
                    $.ajax({
                        url: ITVenture.Helpers.ResolveUrl("~/ScheduledTasks/ScheduledTasks/UpdateWeekday"),
                        data: {
                            Day: day,
                            isActive: e.checked,
                            PeriodicScheduleId: PeriodicScheduleId
                        },
                        success: function(data) {
                            console.log(data.message);
                        }
                    });
                },
                updateMonth: function(e) {
                    var PeriodicScheduleId = e.sender.element.attr("PeriodicScheduleId");
                    var month = e.sender.element.attr("name");
                    var usc = month.indexOf("_");
                    month = month.substr(0, usc);
                    $.ajax({
                        url: ITVenture.Helpers.ResolveUrl("~/ScheduledTasks/ScheduledTasks/UpdateMonth"),
                        data: { Month: month, isActive: e.checked, PeriodicScheduleId: PeriodicScheduleId },
                        success: function (data) {
                            console.log(data.message);
                        }
                    });
                }
            },
            Steps: {
                reorderers: [],
                InitStepView: function(periodicStepId) {
                    var wrap = $('#StepDetails'.concat(periodicStepId.toString()));
                    $(wrap).children(".panel-wrap").on("click", "span.k-i-arrow-chevron-down", function (e) {
                        var contentElement = $(e.target).closest(".widgetbox").find(">div");
                        $(e.target)
                            .removeClass("k-i-arrow-chevron-down")
                            .addClass("k-i-arrow-chevron-up");

                        kendo.fx(contentElement).expand("vertical").stop().play();
                    });

                    //collapse
                    $(wrap).children(".panel-wrap").on("click", "span.k-i-arrow-chevron-up", function (e) {
                        var contentElement = $(e.target).closest(".widgetbox").find(">div");
                        $(e.target)
                            .removeClass("k-i-arrow-chevron-up")
                            .addClass("k-i-arrow-chevron-down");

                        kendo.fx(contentElement).expand("vertical").stop().reverse();
                    });
                },
                CheckReordering: function(e) {
                    var currentName = e.sender.element.attr("id");
                    if (typeof (ITVenture.Pages.TaskScheduler.TaskScheduler.Index.AssignedSchedule.Steps.reorderers[currentName]) === "undefined" ||
                        ITVenture.Pages.TaskScheduler.TaskScheduler.Index.AssignedSchedule.Steps.reorderers[currentName] == null || ITVenture.Pages.TaskScheduler.TaskScheduler.Index.AssignedSchedule.Steps.reorderers[currentName].invalid()) {
                        ITVenture.Pages.TaskScheduler.TaskScheduler.Index.AssignedSchedule.Steps.reorderers[currentName] = ITVenture.Tools.RowReordering.Initialize(currentName, function(entity, newIndex) {
                            entity.StepOrder = newIndex;
                        },true);
                    }
                    ITVenture.Pages.TaskScheduler.TaskScheduler.Index.AssignedSchedule.Steps.reorderers[currentName].styleDraggers();
                },
                toggleReordering: function(e) {
                    var reorderControl = ITVenture.Pages.TaskScheduler.TaskScheduler.Index.AssignedSchedule.Steps.reorderers[$(e.currentTarget).parent().parent().attr("id")];
                    reorderControl.enabled = !reorderControl.enabled;
                    $(e.currentTarget).text(reorderControl.enabled ? "Disable Reorder" : "EnableReorder");
                }
            }
        },
        MainGrid: {
            runTask: function(e) {
                var target = $(e.currentTarget);
                var grid = target.closest(".k-grid").data("kendoGrid");
                var row = target.closest("tr");
                var periodicTaskId = grid.dataItem(row).PeriodicTaskId;
                $.ajax({
                    url: ITVenture.Helpers.ResolveUrl("~/ScheduledTasks/ScheduledTasks/RunTask?periodicTaskId=" + periodicTaskId),
                    success: function (data) {
                        //console.log(data);
                        alert(data);
                    }
                });
            },
            kickable: function(dataItem) {
                return dataItem.Pushable;
            }
        },
        Runs: {
            read: function(e) {
                var periodicTaskId = e.sender.element.attr("PeriodicTaskId");
                var grid = $("#AssignedRuns".concat(periodicTaskId)).data("kendoGrid");
                grid.dataSource.read();
            },
            data: function(periodicTaskId) {
                var retVal = function(filter) {
                    filter.rep = retVal.reports.data("kendoSwitch").value();
                    filter.wrn = retVal.warnings.data("kendoSwitch").value();
                    filter.err = retVal.errors.data("kendoSwitch").value();
                    filter.tx = retVal.text.val();
                };

                retVal.reports = $("[name='Reports"+periodicTaskId+"']");
                retVal.warnings= $("[name='Warnings"+periodicTaskId+"']");
                retVal.errors = $("[name='Errors"+periodicTaskId+"']");
                retVal.text = $("[name='textFilter" + periodicTaskId + "']");
                return retVal;
            }
        }
    }
};