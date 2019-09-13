## Kubectl Version Manager (kvm)

This is a small tool to manage different kubectl versions on your local machine.

Right now, the following operating systems are supported:

- linux x64
- musl based linux x64
- mac os x64
- windows x64

To use the manager, download the most recent version from the releases page and put it somewhere
in your system where you can access it (i.e. where it is in the \$PATH variable.).

After that, you need to refresh the list of versions from github (with `kvm refresh`). This command does
refresh all versions that are available from github. The manager does not call github every time, because
this would use up all your api calls (which are limited to 60 calls per hour per ip).

So `kvm ls-remote` does list all versions that are avaiable and the version number is cached locally.
