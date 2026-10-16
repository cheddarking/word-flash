# Verse data source

`src/WordFlash.Core/Data/verses.json` is generated from the eBible.org King James Version USFM archive.

| Field | Value |
|---|---|
| Edition | King James (Authorized) Version, 1769 text, protocanon, with Strong's numbers (`eng-kjv2006`) |
| Download | https://ebible.org/Scriptures/eng-kjv2006_usfm.zip |
| Source files dated | 2026-09-26 |
| SHA-256 | `5789dcd000d60a554abb4cf9b37ce10a53628bfc686e11ada2d6c981455239c1` |
| Licence | Public domain (per `copr.htm` in the archive). In the United Kingdom, the Crown's letters patent restrict printing; this does not apply elsewhere. |

Words of Jesus come from the archive's `\wj` (red-letter) markers.

## Regenerate

```sh
curl -L -o data/source/eng-kjv2006_usfm.zip https://ebible.org/Scriptures/eng-kjv2006_usfm.zip
dotnet.exe run --project tools/WordFlash.DataGen -- data/source/eng-kjv2006_usfm.zip src/WordFlash.Core/Data/verses.json
```

The generator refuses an archive whose SHA-256 differs from the one above. If eBible.org publishes a new archive, review the change, then update the hash here and in `tools/WordFlash.DataGen/Program.cs`.
