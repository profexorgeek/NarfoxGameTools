# ApiChanges.md

## Required Changes to GameStateService

1. **ID Generation**
   - Add a method to generate a locally unique `ushort` ID.
   - Add a method to combine local ID and owner ID into a `uint` globally unique ID.

2. **Client Tracking**
   - Track local and remote clients, including the authority.

3. **Model Syncing**
   - Support syncing `IEntityData` from network messages similarly to `RequestUpdateModel`.

4. **Interpolation Support**
   - Add hooks or utilities to support value interpolation over time.
