#!/usr/bin/env bash
set -euo pipefail

mordex_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
mordex_configuration="${1:-Release}"
case "$mordex_configuration" in Debug|Release) ;; *) echo 'Usage: build.sh [Debug|Release] [--rebuild]' >&2; exit 2 ;; esac
mordex_build_arguments=(build "$mordex_root/Mordex.sln" --configuration "$mordex_configuration" --nologo --disable-build-servers -m:1)
for mordex_option in "${@:2}"; do
    case "$mordex_option" in
        --rebuild) mordex_build_arguments+=(--no-incremental) ;;
        *) echo 'Usage: build.sh [Debug|Release] [--rebuild]' >&2; exit 2 ;;
    esac
done

dotnet "${mordex_build_arguments[@]}"
mkdir -p -- "$mordex_root/artifacts/bin"
for mordex_project in "$mordex_root"/src/*; do
    [[ -d "$mordex_project" ]] || continue
    mordex_name="$(basename -- "$(ls "$mordex_project"/*.csproj | head -n 1)" .csproj)"
    for mordex_extension in dll pdb xml; do
        mordex_file="$mordex_project/bin/$mordex_configuration/net10.0-windows/$mordex_name.$mordex_extension"
        if [[ -f "$mordex_file" ]]; then
            cp -- "$mordex_file" "$mordex_root/artifacts/bin/"
        fi
    done
done
echo "Built assemblies: $mordex_root/artifacts/bin"
