sdk_bin="/usr/share/dotnet/sdk/2.0.0/"
build_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
RuntimeIdentifiers="win7-x86;win7-x64;win8-x86;win8-x64;win81-x86;win81-x64;win10-x86;win10-x64;osx.10.10-x64;osx.10.11-x64;osx.10.12-x64;centos.7-x86;centos-x64;centos.7-x64;ubuntu.14.04-x64;ubuntu.14.10-x64;ubuntu.15.04-x64;ubuntu.15.10-x64;ubuntu.16.04-x64;ubuntu.16.10-x64"

echo work build dirs: $build_dir
cd "$build_dir"

OLD_IFS="$IFS"
IFS=";"
arr=($RuntimeIdentifiers)
IFS="$OLD_IFS"
for s in ${arr[@]}
do
	"$sdk_bin/dotnet" publish -c release --self-contained -f netcoreapp2.0 -r $s -o "$build_dir/build/$s"
done
exit
