![Example Screenshot of Application](https://raw.githubusercontent.com/jlgaffney/neo-gui-wpf/master/blobs/example_screenshot.png)

!!! APPLICATION IS STILL BEING TESTED !!!
=========================================
!!! PLEASE DO NOT USE TO CONDUCT TRANSACTIONS ON NEO MAINNET !!! USE AT OWN RISK !!!
====================================================================================

Project Setup
=============

On Linux:
=========
`yum install leveldb-devel`

On Windows:
===========

To build and run locally, you need to clone and build https://github.com/neo-project/leveldb first, 
then copy `libleveldb.dll` to the working directory (i.e. /bin/Debug, /bin/Release)

Note: When building, the project file settings must be changed from static library (lib) to dynamic linked library (dll).
