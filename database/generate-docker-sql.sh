#!/usr/bin/env bash
# Generates chinook.docker.sql from the canonical chinook_with_sprocs-id-pk.sql.
#
# The canonical script opens with a CREATE DATABASE [Chinook] statement whose ON PRIMARY and
# LOG ON clauses point at Windows paths. Those fail on the Linux SQL Server container, so this
# script replaces that one statement, from "CREATE DATABASE [Chinook]" through the line ending in
# "WITH CATALOG_COLLATION = DATABASE_DEFAULT", with a bare CREATE DATABASE [Chinook].
# Every other line, including the ALTER DATABASE block, is left alone, with one exception:
#
# The canonical script's AlbumWithArtistName view still selects Album.AlbumId and joins on
# Artist.ArtistId, columns that were renamed to Id in this id-pk variant. On a clean load the view
# fails with "Invalid column name" and never exists. The generator rewrites that one SELECT so the
# view compiles against the Id columns while keeping its original output column names
# (AlbumId, Title, ArtistId, Name). Both edits are guarded and fail loudly if the text has moved.
#
# The canonical file is UTF-16LE with a BOM and CRLF line endings. The output keeps that encoding
# and those line endings, so a diff of the two files shows only the replaced statement.
set -euo pipefail

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
src="$here/chinook_with_sprocs-id-pk.sql"
dst="$here/chinook.docker.sql"

[[ -f "$src" ]] || { echo "ERROR: canonical script not found: $src" >&2; exit 1; }

python3 - "$src" "$dst" <<'PY'
import re, sys

src, dst = sys.argv[1], sys.argv[2]
raw = open(src, "rb").read()

if raw.startswith(b"\xff\xfe"):
    encoding = "utf-16"
elif raw.startswith(b"\xef\xbb\xbf"):
    encoding = "utf-8-sig"
else:
    encoding = "utf-8"
text = raw.decode(encoding)

pattern = re.compile(
    r"CREATE DATABASE \[Chinook\][^\n]*\r?\n"     # the opening line
    r"(?:(?!GO\r?\n)[^\n]*\r?\n)*?"               # the file clauses, never crossing a GO
    r"[^\n]*WITH CATALOG_COLLATION = DATABASE_DEFAULT[ \t]*(?=\r?\n)"
)
matches = pattern.findall(text)
if len(matches) != 1:
    sys.exit(f"ERROR: expected exactly one CREATE DATABASE [Chinook] ... WITH CATALOG_COLLATION = DATABASE_DEFAULT "
             f"statement in {src}, found {len(matches)}. The canonical script has changed; update this generator.")
if "C:\\" not in matches[0]:
    sys.exit("ERROR: the CREATE DATABASE statement no longer contains Windows paths; check whether this generator is still needed.")

out = pattern.sub("CREATE DATABASE [Chinook]", text, count=1)

view_pattern = re.compile(
    r"SELECT +dbo\.Album\.AlbumId, dbo\.Album\.Title, dbo\.Album\.ArtistId, dbo\.Artist\.Name(\r?\n)"
    r"FROM +dbo\.Album INNER JOIN(\r?\n)"
    r"( +)dbo\.Artist ON dbo\.Album\.ArtistId = dbo\.Artist\.ArtistId"
)
view_matches = view_pattern.findall(out)
if len(view_matches) != 1:
    sys.exit(f"ERROR: expected exactly one broken AlbumWithArtistName SELECT in {src}, found {len(view_matches)}. "
             "If the canonical script fixed the view, remove this step from the generator.")
out = view_pattern.sub(
    lambda m: "SELECT        dbo.Album.Id AS AlbumId, dbo.Album.Title, dbo.Album.ArtistId, dbo.Artist.Name" + m.group(1)
            + "FROM            dbo.Album INNER JOIN" + m.group(2)
            + m.group(3) + "dbo.Artist ON dbo.Album.ArtistId = dbo.Artist.Id",
    out, count=1)
open(dst, "wb").write(out.encode(encoding))
print(f"Wrote {dst} ({encoding}); replaced a {matches[0].count(chr(10)) + 1}-line CREATE DATABASE statement "
      "and repaired the AlbumWithArtistName view.")
PY
