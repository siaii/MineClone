public class GrassBlockItem : InventoryItem
{
    public override bool IsPlaceable => true;
    
    public override BlockTypes PlacedBlock => BlockTypes.GRASS;
    
    public override void RightClickFunction()
    {
                
    }
}
