pkgname=katana-hyprland
pkgver=1.0.0
pkgrel=1
pkgdesc="C# interface for hyprctl"
arch=('x86_64')
url='https://github.com/MicrogamerCz/Katana.Hyprland'
license=('GPL-3.0-or-later')
depends=('dotnet-runtime>=8.0.0' 'hyprland')
source=("git+https://github.com/MicrogamerCz/Katana.Hyprland")
sha256sums=('SKIP')

build() {
    cd Katana.Hyprland
    dotnet publish -c Release -r linux-x64
}
package() {
    cd Katana.Hyprland
    install -Dm 644 LICENSE -t "${pkgdir}/usr/share/licenses/${pkgname}"
    cd bin/Release/net8.0/linux-x64/publish
    rm *.pdb

    for file in "$(pwd)"/*; do
        if [[ -f "$file" ]]; then
            install -Dm644 "$file" "$pkgdir/usr/lib/katana/$(basename "$file")"
        fi
    done
}
