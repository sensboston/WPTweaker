# WPTweaker
It's a first XML-based registry tweaker for the Windows Phone 8.1 and Windows 10 Mobile.
You may easily add new hacks by modifying XML data file.

This app should work on the interop-unlocked Lumias and Samsung handsets ONLY.

The WPTweaker's user interface is pretty simple and doesn't require explanation. 
The XML data file format is kinda more complicated (but nothing close to the "rocket science" of course )

So, here an example of the Tweaks.xml file:
```
<?xml version="1.0" encoding="utf-8" ?>
<tweaks>

  <contributors>
    <contributor>sensboston</contributor>
  </contributors>

  <tweak category="User interface" name="Haptic feedback (toggle)" type="toggle" description="Enable or disable haptic feedback" reboot="true">
    <entry path="HKEY_LOCAL_MACHINESystemTouchButtons" name="Vibrate" type="dword" default="0">
      <value>1</value>
    </entry>
  </tweak>

  <tweak category="User interface" name="Haptic feedback (input)" type="input" description="Enable or disable haptic feedback" reboot="true">
    <entry path="HKEY_LOCAL_MACHINESystemTouchButtons" name="Vibrate" type="dword" default="0">
      <value>1</value>
    </entry>
  </tweak>

  <tweak category="User interface" name="Haptic feedback (enum)" type="enum" description="Enable or disable haptic feedback" reboot="true">
    <entry path="HKEY_LOCAL_MACHINESystemTouchButtons" name="Vibrate" type="dword" default="0">
      <value name="Enabled">1</value>
      <value name="Disabled">0</value>
    </entry>
  </tweak>

  <!-- this is an example of the complex tweak with the multiple registry entries -->
  <tweak category="System" name="Enable internet sharing" type="toggle" description="Enable internet sharing, disabled by cell provider" reboot="true">
    <entry path="HKEY_LOCAL_MACHINESystemControlSet001ServicesICSSVCSettings" name="Enabled" type="dword" default="0">
      <value>1</value>
    </entry>
    <entry path="HKEY_LOCAL_MACHINESystemControlSet001ServicesICSSVCSettings" name="EntitlementRequired" type="dword" default="1">
      <value>0</value>
    </entry>
    <entry path="HKEY_LOCAL_MACHINESoftwareMicrosoftSettings{1DEF9B7D-2322-40eb-A007-16A75D5CDA6F}" name="@" type="string">
      <value>Soft AP</value>
    </entry>
    <entry path="HKEY_LOCAL_MACHINESoftwareMicrosoftSettings{1DEF9B7D-2322-40eb-A007-16A75D5CDA6F}" name="Location" type="string">
      <value>app://5B04B775-356B-4AA0-AAF8-6491FFEA5629/Default</value>
    </entry>
    <entry path="HKEY_LOCAL_MACHINESoftwareMicrosoftSettings{1DEF9B7D-2322-40eb-A007-16A75D5CDA6F}" name="Plugin" type="string">
      <value>{09c51652-2cbc-49d5-883e-20606f9a47ff}</value>
    </entry>
    <entry path="HKEY_LOCAL_MACHINESoftwareMicrosoftSettings{1DEF9B7D-2322-40eb-A007-16A75D5CDA6F}" name="QuickSettingsIconURI" type="string">
      <value>res://UIXMobileAssets{ScreenResolution}!actioncenter.hotspot.tier25.png</value>
    </entry>
    <entry path="HKEY_LOCAL_MACHINESoftwareMicrosoftSettings{1DEF9B7D-2322-40eb-A007-16A75D5CDA6F}" name="QuickSettingsTitle" type="string">
      <value>@windowssystem32Settings3Res.dll,-535</value>
    </entry>
    <entry path="HKEY_LOCAL_MACHINESoftwareMicrosoftSettings{1DEF9B7D-2322-40eb-A007-16A75D5CDA6F}" name="Title" type="string">
      <value>@windowssystem32Settings3Res.dll,-242</value>
    </entry>
    <entry path="HKEY_LOCAL_MACHINESoftwareMicrosoftSettings{1DEF9B7D-2322-40eb-A007-16A75D5CDA6F}" name="Type" type="dword" default="0">
      <value>1</value>
    </entry>
    <entry path="HKEY_LOCAL_MACHINESoftwareMicrosoftSettings{69DAA7D1-09EA-4eae-A67E-56E4B0B4CA5B}SecureItems" name="{1DEF9B7D-2322-40eb-A007-16A75D5CDA6F}" type="dword">
      <value>b0</value>
    </entry>
    <entry path="HKEY_LOCAL_MACHINESoftwareMicrosoftSettingsQuickSettingElements" name="{1DEF9B7D-2322-40eb-A007-16A75D5CDA6F}" type="dword">
      <value>7</value>
    </entry>
  </tweak>
</tweaks>
```

I'll try to explain this XML: 

```<contributors>``` element is a tweak contributors list, I'll be glad to add you to this list.

Element ```<tweak>``` must have some attributes and may have some...
required attributes
- ```category="User interface"```: it's a tweak category, all tweaks will be grouped by categories, and these categories become a pivot pages headers
- ```name="Touch buttons intensity"```: short tweak description
- ```type="enum"```: tweak type. There are 3 types of tweaks currently serving: toggle, input and enum
optional attributes
- ```description="Increase or decrease intensity of touch buttons"```: detailed tweak description
- ```reboot="true"```: reboot is required for this tweak?
- ```min="100", max="1000"```: minimal and maximal value limits (for numeric input, currently not implemented)

Element ```<entry>``` represents registry entry.
required attributes
- ```path="HKLMSoftwareMicrosoftDeviceRegInstall"``` : registry key path
- ```name="MaxUnsignedApp"```: registry key name
- ```type="dword"```: registry data type, can be "dword", "qword", "string", "strings" and "binary"
- ```default="0000000A"``` : default registry value. Can be omitted but definitely good to have one...
optional attributes
- ```comparer=">"```: determines how to check tweak state. Logical operation for the value comparer (with default value). By default it's "!=" (not equal), also can be ">" or "<"

Element ```<value>``` it's a registry entry value.
This element may have an optional attribute "name", to specify how this value should appear in the combo box.

▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

Since version 1.2.0 I've added ability to customize application's look, it can be done by adding a few element to the XML data file (Tweaks.xml)
```
<theme name="Blue Waves">
    <AppHeaderBackgroundBrush>#0F1F2E</AppHeaderBackgroundBrush>
    <AppHeaderForegroundBrush>#C2D1E0</AppHeaderForegroundBrush>"
    <AppHeaderFontSize>32</AppHeaderFontSize>"
    <PageHeaderBackgroundBrush>#0F1F2E</PageHeaderBackgroundBrush>
    <PageHeaderForegroundBrush>#C2D1E0</PageHeaderForegroundBrush>
    <PageHeaderFontSize>32</PageHeaderFontSize>
    <TweakHeaderBackgroundBrush>#24476B</TweakHeaderBackgroundBrush>
    <TweakHeaderForegroundBrush>#C2EBFF</TweakHeaderForegroundBrush>
    <TweakHeaderFontSize>22</TweakHeaderFontSize>
    <TweakDescriptionForegroundBrush>#C2EBFF</TweakDescriptionForegroundBrush>
    <TweakDescriptionFontSize>14</TweakDescriptionFontSize>
    <TweakEvenBackgroundBrush>#0F1F2E</TweakEvenBackgroundBrush>
    <TweakOddBackgroundBrush>#14293D</TweakOddBackgroundBrush>
  </theme>
```  
You may use standard WP theme resources (see below) or custom color values in #argb format ('a' - transparency - can be omitted). 

To apply color theme settings, do the following:
* add theme elements to your Tweaks.xml (copy it from the github or from phone first) right under the <tweaks> tag;
* load changed file to WPTweaker
* exit application - it's important!
* start app again and enjoy!

After applying new theme, you can update Tweaks.xml file via http, custom colors will remain. 

To reset app theme to default, just add empty element <theme /> to your xml and load the file.

▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

If you want to contribute tweaks and hacks to the project, I'll be glad to list your name/nick/email in the app's "about box"! But please check your hacks first, and ask me (here, in this thread) if you have any questions.

If you like this project, you may buy me a couple bottles of beer by donating, or by installing and rating "5 stars" my apps from the store 

Credits: I wanna say big "thanks" to @vcfan and @-W_O_L_F- for their work (I used their RPC libraries), and to all whitehats from xda-dev!
