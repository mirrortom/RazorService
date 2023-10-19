### razor编译库
更新时注意事项

1. razorserviceVSIX插件项目使用此库,具体引用的是发布目录(bin\Release\net6.0\publish\win-x64\)的dll文件.
2. 每当库更新后,VSIX插件项目的razor win服务项目,需要重新发布,因为项目发布后使用dll是复制过来的,不会是项目中引用的那个.所以,razor库更新后,原来的插件服务依然使用的还是旧的库,所以需要再次发布一次,或者将新的razorservice.dll,覆盖到服务目录.