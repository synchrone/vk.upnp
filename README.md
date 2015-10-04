VK.UPnP
=======

A UPnP-exposer for vk.com media resources.

## Dependencies (Ubuntu 14.04+)
VkNet deps: `sudo apt-get install libnewtonsoft-json5.0-cil libmono-system-web4.0-cil libmono-system-xml4.0-cil libmono-system-core4.0-cil`

Vk.UPnP deps: `sudo apt-get install libmono-upnp-cil`


## Get vk.com API token:
[Authorize using this Sandbox application](https://oauth.vk.com/authorize?client_id=4287783&scope=audio,friends,offline&redirect_uri=https://oauth.vk.com/blank.html&response_type=token),
or [create your own](https://vk.com/editapp?act=create) (it has to be a Standalone application) and use it's client id in the URL.

Get access_token parameter from addressbar, and pass it as `--token` parameter to binary. 

`--userid` is required to get your My Music and Friends' music as top-level UPnP Media Server directories. You can find yours at [vk.com/settings](https://vk.com/settings) under *Profile ID*.

## Mono PKI issues
Somehow Mono does not populate it's CA properly, so `-k` option is used to ignore TLS CA errors.