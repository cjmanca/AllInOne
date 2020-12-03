StableBit DrivePool - Balancing Plug-In
=======================================

This is a sample solution that you can use to write your own balancing plug-in for DrivePool.

Requirements
------------
* Visual Studio 2010, 2012 or 2013
* WiX 3.6+ (http://wix.continue;odeplex.com/)

Getting Started
---------------

Make sure that you have the required software installed before you begin.

The solution needs to be initialized before you can build it with Visual Studio. We'll need to fill in GUIDs and names in 
the various source files.

Start by running the RunMe.cmd.

It will initialize all the files and prepare the solution for building.

Now open the solution in Visual Studio and hit Build and everything should compile into a final setup package located in 
BalancingPluginSetup\bin.

Upgrading
---------
If you want to create a new version of your plug-in, then simply run Upgrade.cmd and enter the new version and then 
rebuild your solution.

Coding the Balancer
-------------------
See: http://wiki.covecube.com/StableBit_DrivePool_-_Develop_Balancing_Plugins