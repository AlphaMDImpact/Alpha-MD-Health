window: customJSInterop = {
    clickedID: undefined,
    focusOutFromDiv: function (id) {
        ///if id is passed in ID field
        if (customJSInterop.clickedID != undefined && customJSInterop.clickedID.id == undefined) {
            customJSInterop.closeDropDown(customJSInterop.clickedID);
        }
        ///if resourcekey is passed in ID field
        if (customJSInterop.clickedID != undefined && customJSInterop.clickedID.id != undefined) {
            customJSInterop.closeDropDown(customJSInterop.clickedID.id);
        }
        customJSInterop.clickedID = id;
        document.addEventListener('click', customJSInterop.myFunction);
        if (customJSInterop.clickedID.id != undefined) {
            document.getElementById(customJSInterop.clickedID.id + "ListItems").className = "select-menu show";
            if (!customJSInterop.isDomDataNull(customJSInterop.clickedID.id + "CloseIcon")) {
                document.getElementById(customJSInterop.clickedID.id + "CloseIcon").className = "close-icon show";
            }
        }
        else {
            document.getElementById(customJSInterop.clickedID + "ListItems").className = "select-menu show";
            if (!customJSInterop.isDomDataNull(customJSInterop.clickedID + "CloseIcon")) {
                document.getElementById(customJSInterop.clickedID + "CloseIcon").className = "close-icon show";
            }
        }
        //var pageHeight= $(id).closest('.outer-div').height();
        // if (isUP) {
        //     document.getElementById(customJSInterop.clickedID.id + "ListItems").className = "select-menu show drop-up";
        // }

    },
    myFunction: function (event) {
        if (customJSInterop.clickedID != undefined && customJSInterop.clickedID.id == undefined) {
            var clickedElement = document.getElementById(customJSInterop.clickedID);
            if (clickedElement) {
                var isClickInsideElement = clickedElement.contains(event.target);
                if (!isClickInsideElement) {
                    customJSInterop.closeDropDown(customJSInterop.clickedID);
                    document.removeEventListener('click', customJSInterop.myFunction);
                }
            }
        }
        if (customJSInterop.clickedID != undefined && customJSInterop.clickedID.id != undefined) {
            var isClickInsideElement = customJSInterop.clickedID.contains(event.target);
            if (!isClickInsideElement) {
                if (customJSInterop.clickedID.id != undefined) {
                    customJSInterop.closeDropDown(customJSInterop.clickedID.id);
                }
                else {
                    customJSInterop.closeDropDown(customJSInterop.clickedID);
                }
                document.removeEventListener('click', customJSInterop.myFunction);
            }
        }
    },
    closeDropDown: function (id) {
        document.getElementById(id + "ListItems").className = "select-menu hide";
        if (!customJSInterop.isDomDataNull(id + "CloseIcon")) {
            document.getElementById(id + "CloseIcon").className = "close-icon hide";
        }
        customJSInterop.clickedID = undefined;
    },
    focusOutFromDateTime: function (id) {
        document.addEventListener('click', function (event) {
            if (id !== undefined && id !== null) {
                var isClickInsideElement = id.contains(event.target);
                if (!isClickInsideElement) {
                    DotNet.invokeMethodAsync('AlphaMDHealth.WebClient', 'CloseDateTime', id.id);
                    id = undefined;
                }
            }
        });
    },
    isDomDataNull: function (id) {
        var domElement = document.getElementById(id);
        return (domElement == undefined || domElement == null);
    },
    logIntoBrowser: function (message) {
        console.log(message);
    }
}