namespace POSApi.Domain.Enums;

/// <summary>
/// Represents the general category of measurement
/// </summary>
public enum UnitType
{
    /// <summary>
    /// Weight/Mass measurements (kg, g, lb, oz, etc.)
    /// </summary>
    Weight = 1,
    
    /// <summary>
    /// Volume/Liquid measurements (ml, l, gal, oz, etc.)
    /// </summary>
    Volume = 2,
    
    /// <summary>
    /// Length/Distance measurements (cm, m, in, ft, etc.)
    /// </summary>
    Length = 3,
    
    /// <summary>
    /// Area measurements (sq cm, sq m, sq ft, etc.)
    /// </summary>
    Area = 4,
    
    /// <summary>
    /// Count/Discrete units (piece, each, dozen, box, etc.)
    /// </summary>
    Count = 5,
    
    /// <summary>
    /// Time measurements (second, minute, hour, etc.)
    /// </summary>
    Time = 6,
    
    /// <summary>
    /// Temperature measurements (celsius, fahrenheit, kelvin)
    /// </summary>
    Temperature = 7
}

/// <summary>
/// Standard weight/mass units
/// </summary>
public enum WeightUnit
{
    // Metric
    Milligram = 1,
    Gram = 2,
    Kilogram = 3,
    MetricTon = 4,
    
    // Imperial
    Ounce = 101,
    Pound = 102,
    Stone = 103,
    Ton = 104
}

/// <summary>
/// Standard volume/liquid units
/// </summary>
public enum VolumeUnit
{
    // Metric
    Milliliter = 1,
    Liter = 2,
    CubicMeter = 3,
    
    // Imperial/US
    FluidOunce = 101,
    Cup = 102,
    Pint = 103,
    Quart = 104,
    Gallon = 105,
    
    // UK Imperial
    UKFluidOunce = 201,
    UKPint = 202,
    UKQuart = 203,
    UKGallon = 204
}

/// <summary>
/// Standard length/distance units
/// </summary>
public enum LengthUnit
{
    // Metric
    Millimeter = 1,
    Centimeter = 2,
    Meter = 3,
    Kilometer = 4,
    
    // Imperial
    Inch = 101,
    Foot = 102,
    Yard = 103,
    Mile = 104
}

/// <summary>
/// Standard count/discrete units
/// </summary>
public enum CountUnit
{
    Piece = 1,
    Each = 2,
    Pair = 3,
    Dozen = 4,
    Gross = 5, // 144 pieces
    
    // Packaging units
    Pack = 101,
    Box = 102,
    Case = 103,
    Pallet = 104,
    
    // Bulk units
    Bag = 201,
    Sack = 202,
    Barrel = 203,
    Bundle = 204
}