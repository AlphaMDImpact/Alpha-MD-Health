
window.registerDotNetObject = function (dotNetObject) {
    window.dotNetObject = dotNetObject;
};

function OpenRazorpayPopup(orderid) {
    const root = document.documentElement;
    const primaryAppColor = getComputedStyle(root).getPropertyValue('--primary-app-color');

    var options = {
        "description": "",
        "order_id": orderid,
        "prefill": {
        },
        "readonly": { 'email': false, 'contact': false },
        "notes": {
            "address": "",
            "merchant_order_id": "OBU8f1kDQoKKSk",
        },
        "theme": {
            "color": primaryAppColor
        }
    }

    //options.theme.image_padding = false;
    options.handler = function (response) {
        if (window.dotNetObject) {
            window.dotNetObject.invokeMethodAsync('RazorPayResponse', response.razorpay_payment_id, response.razorpay_signature, orderid);
        }
    };
    options.modal = {
        ondismiss: function () {
            if (window.dotNetObject) {
                window.dotNetObject.invokeMethodAsync('OnRazorpayPopupClosed', orderid);
            }
        },
        // Boolean indicating whether pressing escape key should close the checkout form. (default: true)
        escape: false,
        // Boolean indicating whether clicking translucent blank space outside checkout form should close the form. (default: false)
        backdropclose: false
    };

    var rzp = new Razorpay(options);
    rzp.open();
}