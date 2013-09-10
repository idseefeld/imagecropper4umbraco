angular.module("umbraco")
.controller("idseefeld.ImageCropperExtended",
//inject Umbraco’s assetsService
function ($scope, assetsService) {
	//tell the assetsService to load the markdown.editor libs from
	//the markdown editor plugin folder
	assetsService
	.load([
		"/App_Plugins/ImageCropperExtended/lib/json2.js",
		"/App_Plugins/ImageCropperExtended/lib/jCropScript.js",
		"/App_Plugins/ImageCropperExtended/lib/imageCropperScript.js"
	])
	.then(function () {
		//this function will execute when all dependencies have loaded
		alert("editor dependencies loaded");
		initImageCropper('body_cropBox_1375', 'body_ctl18', 'body_ctl19');
	});
	//load the separate css for the editor to avoid it blocking our js loading
	assetsService.loadCss("/App_Plugins/ImageCropperExtended/lib/jCropCSS.css");
});