#!/usr/bin/env bash
# Builds every example solution under 01- through 05- in Release and prints a pass/fail table.
# A folder containing a .skip-build marker is skipped, along with any solution inside it.
# Projects inside a solution can also carry a .skip-build marker; see skip_project below.
set -uo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$root"

results=()
failed=0

has_skip_marker() {
  # True if the given directory, or any ancestor up to the repo root, has a .skip-build marker.
  local dir="$1"
  # Paths are absolute (find runs from $root), so this walk always reaches $root.
  while [[ "$dir" != "$root" && "$dir" != "/" ]]; do
    [[ -f "$dir/.skip-build" ]] && return 0
    dir="$(dirname "$dir")"
  done
  return 1
}

while IFS= read -r sln; do
  dir="$(dirname "$sln")"
  name="${sln#"$root"/}"
  if has_skip_marker "$dir"; then
    results+=("SKIP|$name")
    continue
  fi

  # Projects with their own .skip-build marker are filtered out of the build.
  skipped_projects=()
  while IFS= read -r marker; do
    skipped_projects+=("$(dirname "$marker")")
  done < <(find "$dir" -mindepth 2 -name .skip-build -not -path '*/bin/*' -not -path '*/obj/*')

  if [[ ${#skipped_projects[@]} -eq 0 ]]; then
    if echo ">> $name" && dotnet build "$sln" -c Release --nologo -v quiet --disable-build-servers; then
      results+=("PASS|$name")
    else
      results+=("FAIL|$name")
      failed=1
    fi
  else
    ok=1
    while IFS= read -r proj; do
      proj_dir="$(dirname "$proj")"
      skip=0
      for s in "${skipped_projects[@]}"; do
        [[ "$proj_dir" == "$s" || "$proj_dir" == "$s"/* ]] && skip=1
      done
      [[ $skip -eq 1 ]] && continue
      echo ">> $proj" && dotnet build "$proj" -c Release --nologo -v quiet --disable-build-servers || ok=0
    done < <(find "$dir" -name '*.csproj' -not -path '*/bin/*' -not -path '*/obj/*' | sort)
    if [[ $ok -eq 1 ]]; then
      results+=("PASS|$name (partial: ${#skipped_projects[@]} project(s) skipped)")
    else
      results+=("FAIL|$name")
      failed=1
    fi
  fi
done < <(find "$root"/0[1-5]-* -name '*.sln' -not -path '*/bin/*' -not -path '*/obj/*' | sort)

echo
printf '%-6s %s\n' "RESULT" "SOLUTION"
printf '%-6s %s\n' "------" "--------"
for r in "${results[@]}"; do
  printf '%-6s %s\n' "${r%%|*}" "${r#*|}"
done

exit $failed
