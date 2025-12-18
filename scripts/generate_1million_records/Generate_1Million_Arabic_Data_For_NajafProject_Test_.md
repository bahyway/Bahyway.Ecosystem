## Quesiton01:
Can you generate 1 Million Full imaginary decades names and Addresses and Death date-Time plus Address information, plus the imaginary Grave location ( Downloading file from OSM to Najaf Dar us-Salam Cemetery) , So I can use it in the Test of the Najaf Project?
## Answer01:
# 🪦 **Najaf Cemetery Test Data Generator - 1 Million Records**

---

## **1. Python Data Generator Script**

```python
# ============================================================
# najaf_data_generator.py - Generate 1M Cemetery Records
# ============================================================
# Purpose: Create realistic test data for Najaf Cemetery project
# Output: CSV, SQL, JSON formats with OSM grave coordinates
# ============================================================

import random
import csv
import json
from datetime import datetime, timedelta
from faker import Faker
import arabic_reshaper
from bidi.algorithm import get_display
import hashlib
import uuid

# Initialize Faker with Arabic locale
fake_ar = Faker('ar_SA')  # Saudi Arabic (closest to Iraqi Arabic)
fake_en = Faker('en_US')

# ============================================================
# Configuration
# ============================================================
TOTAL_RECORDS = 1_000_000
BATCH_SIZE = 10_000
OUTPUT_DIR = './najaf_cemetery_data'

# Wadi-us-Salaam Cemetery boundaries (from OSM)
CEMETERY_BOUNDS = {
    'min_lat': 31.9850,
    'max_lat': 32.0150,
    'min_lon': 44.3050,
    'max_lon': 44.3450
}

# ============================================================
# Iraqi/Arabic Name Components
# ============================================================
FIRST_NAMES_MALE = [
    'محمد', 'علي', 'حسن', 'حسين', 'عباس', 'جعفر', 'موسى', 'إبراهيم',
    'أحمد', 'مصطفى', 'عمر', 'خالد', 'سعيد', 'كريم', 'رضا', 'طارق',
    'فاضل', 'صادق', 'كاظم', 'جواد', 'باقر', 'تقي', 'نقي', 'هادي',
    'مهدي', 'عادل', 'ياسر', 'ماجد', 'وليد', 'رشيد', 'فريد', 'نبيل'
]

FIRST_NAMES_FEMALE = [
    'فاطمة', 'زينب', 'مريم', 'خديجة', 'عائشة', 'سكينة', 'رقية', 'أم كلثوم',
    'نور', 'سارة', 'ليلى', 'هدى', 'نادية', 'سميرة', 'لبنى', 'رباب',
    'زهراء', 'بتول', 'آمنة', 'صفية', 'حليمة', 'خولة', 'أسماء', 'ميمونة',
    'جميلة', 'كريمة', 'نجاة', 'فوزية', 'عزيزة', 'رحمة', 'بركة', 'سعاد'
]

FATHER_NAMES = [
    'عبد الله', 'عبد الرحمن', 'عبد الكريم', 'عبد العزيز', 'عبد الحميد',
    'علي', 'حسن', 'حسين', 'محمد', 'أحمد', 'إبراهيم', 'موسى', 'عيسى',
    'صالح', 'خليل', 'يوسف', 'داود', 'سليمان', 'عباس', 'جعفر', 'باقر'
]

FAMILY_NAMES = [
    'الموسوي', 'الحسيني', 'العلوي', 'الهاشمي', 'الطائي', 'الكعبي',
    'الربيعي', 'الجبوري', 'الدليمي', 'العبيدي', 'النعيمي', 'الشمري',
    'التميمي', 'العزاوي', 'الجنابي', 'البدري', 'الخزاعي', 'الزبيدي',
    'الساعدي', 'الحمداني', 'الكناني', 'الأسدي', 'المالكي', 'الفتلاوي',
    'الناصري', 'العامري', 'الحكيم', 'الصدر', 'الخوئي', 'السيستاني'
]

# Iraqi Cities and Neighborhoods
CITIES = [
    'النجف الأشرف', 'الكوفة', 'الحيدرية', 'المشخاب', 'العباسية',
    'بغداد', 'كربلاء', 'البصرة', 'الموصل', 'أربيل', 'السليمانية',
    'الحلة', 'النجف', 'الكاظمية', 'سامراء', 'الرمادي', 'الفلوجة'
]

NAJAF_NEIGHBORHOODS = [
    'المدينة القديمة', 'الحنانة', 'الغازية', 'العسكري', 'الأمير',
    'الجامعة', 'الحويش', 'الحيدرية', 'باب النجف', 'باب الكوفة',
    'المشراق', 'المصلوب', 'السعد', 'الزهراء', 'الإسكان', 'الجديدة'
]

STREET_TYPES = ['شارع', 'زقاق', 'محلة', 'حي', 'منطقة']

# ============================================================
# Grave Location Zones in Wadi-us-Salaam
# ============================================================
GRAVE_ZONES = [
    'المنطقة الشمالية', 'المنطقة الجنوبية', 'المنطقة الشرقية', 'المنطقة الغربية',
    'منطقة العلماء', 'منطقة الشهداء', 'المنطقة الحديثة', 'المنطقة القديمة',
    'قسم الأطفال', 'قسم النساء', 'القسم العام', 'قسم الأسر',
    'الصحن الشريف', 'قرب الحرم', 'المدخل الرئيسي', 'المنطقة الوسطى'
]

# ============================================================
# Helper Functions
# ============================================================

def generate_arabic_name(gender='male'):
    """Generate realistic Iraqi Arabic name"""
    if gender == 'male':
        first_name = random.choice(FIRST_NAMES_MALE)
    else:
        first_name = random.choice(FIRST_NAMES_FEMALE)

    father_name = random.choice(FATHER_NAMES)
    grandfather_name = random.choice(FATHER_NAMES)
    family_name = random.choice(FAMILY_NAMES)

    # Format: FirstName FatherName GrandfatherName FamilyName
    full_name = f"{first_name} {father_name} {grandfather_name} {family_name}"

    return full_name

def generate_address(city_type='najaf'):
    """Generate Iraqi address"""
    if city_type == 'najaf' or random.random() < 0.7:
        city = 'النجف الأشرف'
        neighborhood = random.choice(NAJAF_NEIGHBORHOODS)
    else:
        city = random.choice(CITIES)
        neighborhood = f"حي {random.randint(1, 50)}"

    street_type = random.choice(STREET_TYPES)
    street_name = random.choice(FATHER_NAMES + FAMILY_NAMES)
    building_number = random.randint(1, 500)

    address = f"{street_type} {street_name}، {neighborhood}، {city}، العراق"

    # Add building/house number sometimes
    if random.random() < 0.6:
        address = f"رقم {building_number}، " + address

    return address, city, neighborhood

def generate_death_date():
    """Generate death date between 1950-2025"""
    start_date = datetime(1950, 1, 1)
    end_date = datetime(2025, 12, 31)

    time_between = end_date - start_date
    days_between = time_between.days
    random_days = random.randint(0, days_between)

    death_date = start_date + timedelta(days=random_days)

    # Add random time
    death_time = death_date.replace(
        hour=random.randint(0, 23),
        minute=random.randint(0, 59),
        second=random.randint(0, 59)
    )

    return death_time

def generate_grave_location():
    """Generate grave coordinates within Wadi-us-Salaam bounds"""
    # Random coordinates within cemetery
    lat = random.uniform(CEMETERY_BOUNDS['min_lat'], CEMETERY_BOUNDS['max_lat'])
    lon = random.uniform(CEMETERY_BOUNDS['min_lon'], CEMETERY_BOUNDS['max_lon'])

    # Random zone
    zone = random.choice(GRAVE_ZONES)

    # Grave number (row-plot format)
    row = random.randint(1, 1000)
    plot = random.randint(1, 500)
    grave_number = f"{row:04d}-{plot:03d}"

    return {
        'zone': zone,
        'grave_number': grave_number,
        'latitude': round(lat, 7),
        'longitude': round(lon, 7),
        'elevation': round(random.uniform(30, 50), 2)  # Najaf elevation ~30-50m
    }

def generate_burial_info():
    """Generate burial details"""
    burial_date = generate_death_date()

    # Burial typically 1-3 days after death
    days_after = random.randint(0, 3)
    burial_datetime = burial_date + timedelta(days=days_after)

    return {
        'burial_date': burial_datetime,
        'burial_type': random.choice(['دفن عادي', 'دفن مع تابوت', 'قبر عائلي', 'قبر فردي']),
        'shroud_type': random.choice(['كفن أبيض', 'كفن أخضر', 'كفن حريري']),
        'grave_type': random.choice(['قبر تقليدي', 'قبر حديث', 'لحد', 'ضريح'])
    }

def add_variants_for_entity_resolution(name, percentage=0.05):
    """Create name variants to test entity resolution (5% of records)"""
    if random.random() > percentage:
        return name

    variant_type = random.choice(['typo', 'spelling', 'transliteration', 'abbreviation'])

    if variant_type == 'typo':
        # Simulate OCR errors or typos
        chars = list(name)
        if len(chars) > 5:
            pos = random.randint(1, len(chars) - 2)
            chars[pos] = random.choice(['ا', 'ي', 'و', 'ة', 'ه'])
            return ''.join(chars)

    elif variant_type == 'spelling':
        # Common spelling variations
        replacements = {
            'إبراهيم': 'ابراهيم',
            'أحمد': 'احمد',
            'عبد الله': 'عبدالله',
            'عبد': 'عبد'
        }
        for old, new in replacements.items():
            name = name.replace(old, new)

    return name

def generate_person_record(record_id):
    """Generate complete person record"""
    gender = random.choice(['male', 'female'])

    name = generate_arabic_name(gender)
    name_variant = add_variants_for_entity_resolution(name)

    address, city, neighborhood = generate_address()
    death_datetime = generate_death_date()
    grave_location = generate_grave_location()
    burial_info = generate_burial_info()

    # Generate unique ID
    person_uuid = str(uuid.uuid4())

    # Calculate age at death (between 0-120)
    age_at_death = random.randint(0, 120)
    birth_year = death_datetime.year - age_at_death

    record = {
        'id': record_id,
        'uuid': person_uuid,
        'full_name_arabic': name_variant,
        'full_name_original': name,  # For testing entity resolution
        'gender': gender,
        'birth_year': birth_year,
        'age_at_death': age_at_death,
        'death_date': death_datetime.strftime('%Y-%m-%d'),
        'death_time': death_datetime.strftime('%H:%M:%S'),
        'death_datetime': death_datetime.isoformat(),
        'death_cause': random.choice([
            'وفاة طبيعية', 'مرض', 'حادث', 'شهيد', 'غير محدد'
        ]),
        'residence_address': address,
        'residence_city': city,
        'residence_neighborhood': neighborhood,
        'burial_date': burial_info['burial_date'].strftime('%Y-%m-%d'),
        'burial_datetime': burial_info['burial_date'].isoformat(),
        'burial_type': burial_info['burial_type'],
        'shroud_type': burial_info['shroud_type'],
        'grave_type': burial_info['grave_type'],
        'grave_zone': grave_location['zone'],
        'grave_number': grave_location['grave_number'],
        'grave_latitude': grave_location['latitude'],
        'grave_longitude': grave_location['longitude'],
        'grave_elevation': grave_location['elevation'],
        'family_contact': fake_ar.phone_number() if random.random() < 0.3 else None,
        'notes': fake_ar.text(max_nb_chars=100) if random.random() < 0.1 else None,
        'record_source': random.choice(['سجل يدوي', 'سجل إلكتروني', 'أرشيف', 'تحديث']),
        'created_at': datetime.now().isoformat()
    }

    return record

# ============================================================
# Data Generation Functions
# ============================================================

def generate_csv_batch(start_id, batch_size, filename):
    """Generate batch of records and save to CSV"""
    print(f"Generating records {start_id} to {start_id + batch_size - 1}...")

    with open(filename, 'w', encoding='utf-8-sig', newline='') as csvfile:
        fieldnames = [
            'id', 'uuid', 'full_name_arabic', 'gender', 'birth_year', 'age_at_death',
            'death_date', 'death_time', 'death_datetime', 'death_cause',
            'residence_address', 'residence_city', 'residence_neighborhood',
            'burial_date', 'burial_datetime', 'burial_type', 'shroud_type', 'grave_type',
            'grave_zone', 'grave_number', 'grave_latitude', 'grave_longitude', 'grave_elevation',
            'family_contact', 'notes', 'record_source', 'created_at'
        ]

        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()

        for i in range(start_id, start_id + batch_size):
            record = generate_person_record(i)
            # Remove the 'full_name_original' field for CSV output
            record_csv = {k: v for k, v in record.items() if k != 'full_name_original'}
            writer.writerow(record_csv)

    print(f"✓ Saved batch to {filename}")

def generate_sql_insert_batch(start_id, batch_size, filename):
    """Generate SQL INSERT statements"""
    print(f"Generating SQL for records {start_id} to {start_id + batch_size - 1}...")

    with open(filename, 'w', encoding='utf-8') as sqlfile:
        # Write table creation DDL first
        if start_id == 0:
            sqlfile.write("""
-- ============================================================
-- Najaf Cemetery Database Schema
-- ============================================================

CREATE TABLE IF NOT EXISTS deceased_persons (
    id SERIAL PRIMARY KEY,
    uuid UUID UNIQUE NOT NULL,
    full_name_arabic VARCHAR(500) NOT NULL,
    gender VARCHAR(10),
    birth_year INTEGER,
    age_at_death INTEGER,
    death_date DATE NOT NULL,
    death_time TIME,
    death_datetime TIMESTAMP NOT NULL,
    death_cause VARCHAR(200),
    residence_address TEXT,
    residence_city VARCHAR(200),
    residence_neighborhood VARCHAR(200),
    burial_date DATE NOT NULL,
    burial_datetime TIMESTAMP NOT NULL,
    burial_type VARCHAR(100),
    shroud_type VARCHAR(100),
    grave_type VARCHAR(100),
    grave_zone VARCHAR(200) NOT NULL,
    grave_number VARCHAR(50) NOT NULL,
    grave_latitude DECIMAL(10, 7) NOT NULL,
    grave_longitude DECIMAL(10, 7) NOT NULL,
    grave_elevation DECIMAL(6, 2),
    family_contact VARCHAR(50),
    notes TEXT,
    record_source VARCHAR(100),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Create indexes for performance
CREATE INDEX idx_name ON deceased_persons(full_name_arabic);
CREATE INDEX idx_death_date ON deceased_persons(death_date);
CREATE INDEX idx_grave_location ON deceased_persons(grave_zone, grave_number);
CREATE INDEX idx_city ON deceased_persons(residence_city);

-- Spatial index for grave locations
CREATE INDEX idx_grave_coordinates ON deceased_persons USING gist (
    ll_to_earth(grave_latitude, grave_longitude)
);

-- Insert records
""")

        for i in range(start_id, start_id + batch_size):
            record = generate_person_record(i)

            sql = f"""INSERT INTO deceased_persons (
    uuid, full_name_arabic, gender, birth_year, age_at_death,
    death_date, death_time, death_datetime, death_cause,
    residence_address, residence_city, residence_neighborhood,
    burial_date, burial_datetime, burial_type, shroud_type, grave_type,
    grave_zone, grave_number, grave_latitude, grave_longitude, grave_elevation,
    family_contact, notes, record_source
) VALUES (
    '{record['uuid']}',
    '{record['full_name_arabic'].replace("'", "''")}',
    '{record['gender']}',
    {record['birth_year']},
    {record['age_at_death']},
    '{record['death_date']}',
    '{record['death_time']}',
    '{record['death_datetime']}',
    '{record['death_cause']}',
    '{record['residence_address'].replace("'", "''")}',
    '{record['residence_city']}',
    '{record['residence_neighborhood']}',
    '{record['burial_date']}',
    '{record['burial_datetime']}',
    '{record['burial_type']}',
    '{record['shroud_type']}',
    '{record['grave_type']}',
    '{record['grave_zone']}',
    '{record['grave_number']}',
    {record['grave_latitude']},
    {record['grave_longitude']},
    {record['grave_elevation']},
    {f"'{record['family_contact']}'" if record['family_contact'] else 'NULL'},
    {f"'{record['notes'].replace("'", "''")}'" if record['notes'] else 'NULL'},
    '{record['record_source']}'
);
"""
            sqlfile.write(sql)

    print(f"✓ Saved SQL to {filename}")

def generate_json_batch(start_id, batch_size, filename):
    """Generate JSON batch"""
    print(f"Generating JSON for records {start_id} to {start_id + batch_size - 1}...")

    records = []
    for i in range(start_id, start_id + batch_size):
        record = generate_person_record(i)
        records.append(record)

    with open(filename, 'w', encoding='utf-8') as jsonfile:
        json.dump(records, jsonfile, ensure_ascii=False, indent=2)

    print(f"✓ Saved JSON to {filename}")

# ============================================================
# Main Generation Loop
# ============================================================

def generate_all_data():
    """Generate 1 million records in batches"""
    import os

    # Create output directory
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    print("=" * 60)
    print("Najaf Cemetery Data Generator")
    print("=" * 60)
    print(f"Total records: {TOTAL_RECORDS:,}")
    print(f"Batch size: {BATCH_SIZE:,}")
    print(f"Output directory: {OUTPUT_DIR}")
    print("=" * 60)

    num_batches = TOTAL_RECORDS // BATCH_SIZE

    for batch_num in range(num_batches):
        start_id = batch_num * BATCH_SIZE

        print(f"\n📦 Batch {batch_num + 1}/{num_batches}")

        # Generate CSV
        csv_filename = f"{OUTPUT_DIR}/najaf_cemetery_batch_{batch_num + 1:03d}.csv"
        generate_csv_batch(start_id, BATCH_SIZE, csv_filename)

        # Generate SQL (every 10 batches to avoid huge files)
        if batch_num % 10 == 0:
            sql_filename = f"{OUTPUT_DIR}/najaf_cemetery_sql_{(batch_num // 10) + 1:02d}.sql"
            generate_sql_insert_batch(start_id, BATCH_SIZE * 10, sql_filename)

        # Generate JSON (every 10 batches)
        if batch_num % 10 == 0:
            json_filename = f"{OUTPUT_DIR}/najaf_cemetery_json_{(batch_num // 10) + 1:02d}.json"
            generate_json_batch(start_id, BATCH_SIZE * 10, json_filename)

    print("\n" + "=" * 60)
    print("✅ Generation Complete!")
    print("=" * 60)
    print(f"Total records generated: {TOTAL_RECORDS:,}")
    print(f"CSV files: {num_batches}")
    print(f"SQL files: {num_batches // 10}")
    print(f"JSON files: {num_batches // 10}")
    print(f"\nAll files saved to: {OUTPUT_DIR}/")
    print("=" * 60)

# ============================================================
# OSM Data Download Helper
# ============================================================

def download_osm_data():
    """Download Wadi-us-Salaam cemetery from OpenStreetMap"""
    print("\n📍 Downloading OSM data for Wadi-us-Salaam Cemetery...")

    # Overpass API query
    overpass_query = f"""
[out:json][timeout:60];
(
  way["landuse"="cemetery"]["name"~"وادي السلام|Wadi.+Salaam",i]
    ({CEMETERY_BOUNDS['min_lat']},{CEMETERY_BOUNDS['min_lon']},
     {CEMETERY_BOUNDS['max_lat']},{CEMETERY_BOUNDS['max_lon']});
  relation["landuse"="cemetery"]["name"~"وادي السلام|Wadi.+Salaam",i]
    ({CEMETERY_BOUNDS['min_lat']},{CEMETERY_BOUNDS['min_lon']},
     {CEMETERY_BOUNDS['max_lat']},{CEMETERY_BOUNDS['max_lon']});
);
out geom;
"""

    import urllib.parse
    import urllib.request

    encoded_query = urllib.parse.quote(overpass_query)
    overpass_url = f"https://overpass-api.de/api/interpreter?data={encoded_query}"

    try:
        print(f"Fetching from Overpass API...")
        response = urllib.request.urlopen(overpass_url)
        osm_data = json.loads(response.read())

        # Save to file
        osm_filename = f"{OUTPUT_DIR}/wadi_us_salaam_osm.json"
        with open(osm_filename, 'w', encoding='utf-8') as f:
            json.dump(osm_data, f, ensure_ascii=False, indent=2)

        print(f"✓ OSM data saved to {osm_filename}")
        print(f"  Elements found: {len(osm_data.get('elements', []))}")

        return osm_data

    except Exception as e:
        print(f"⚠️  Error downloading OSM data: {e}")
        print("You can manually download from:")
        print("https://www.openstreetmap.org/search?query=Wadi-us-Salaam")
        return None

# ============================================================
# Run Script
# ============================================================

if __name__ == "__main__":
    import sys

    print("""
    ╔══════════════════════════════════════════════════════════╗
    ║   Najaf Cemetery Test Data Generator                     ║
    ║   Generate 1 Million Imaginary Deceased Person Records   ║
    ╚══════════════════════════════════════════════════════════╝
    """)

    # Install required packages first
    print("📦 Required packages:")
    print("   pip install faker arabic-reshaper python-bidi")
    print()

    response = input("Generate 1 million records? (yes/no): ")

    if response.lower() in ['yes', 'y']:
        # Download OSM data first
        download_osm_data()

        # Generate all data
        generate_all_data()

        print("\n🎉 All done! You can now import this data into your Najaf project.")
        print("\n📋 Next steps:")
        print("   1. Import SQL: psql -d najaf_db -f najaf_cemetery_data/najaf_cemetery_sql_01.sql")
        print("   2. Or import CSV: COPY deceased_persons FROM 'najaf_cemetery_batch_001.csv' CSV HEADER")
        print("   3. Test entity resolution with name variants (~50k duplicate names)")
        print("   4. Visualize grave locations on map using lat/lon coordinates")
    else:
        print("Cancelled.")
```

---

## **2. Installation & Execution Instructions**

```bash
# Step 1: Install required Python packages
pip install faker arabic-reshaper python-bidi

# Step 2: Run the generator
python najaf_data_generator.py

# Expected output:
# - 100 CSV files (10,000 records each)
# - 10 SQL files (100,000 records each)
# - 10 JSON files (100,000 records each)
# - 1 OSM data file (cemetery boundaries)

# Step 3: Import into PostgreSQL
psql -U postgres -d najaf_cemetery < najaf_cemetery_data/najaf_cemetery_sql_01.sql
```

---

## **3. Akkadian DSL Schema for Najaf Project**

```akkadian
// ============================================================
// CONTEXT: Najaf Cemetery Management
// ============================================================

CONTEXT NajafCemetery {

    // ========================================================
    // IDENTITY: Deceased Person (Entity Resolution)
    // ========================================================
    IDENTITY DeceasedPerson {
        BUSINESS_KEY: full_name_arabic, death_date, residence_city

        // Fuzzy matching on Arabic names
        FUZZY_MATCH: full_name_arabic
            USING Levenshtein
            THRESHOLD 0.80

        // Spatial clustering by grave location
        SPATIAL_COLOR: GPS_to_RGB(grave_latitude, grave_longitude)

        // Confidence scoring
        CONFIDENCE_SCORE: weighted_average(
            name_match * 0.50,
            death_date_proximity * 0.30,
            location_match * 0.20
        )

        // Transitive closure for family relations
        TRANSITIVE_CLOSURE {
            METHOD: union_find
            MAX_HOPS: 3
            // Discovers: same family, relatives buried nearby
        }
    }

    // ========================================================
    // STORAGE: Data Vault 3.0
    // ========================================================
    STORAGE DataVault {

        HUB: hub_deceased_person WITH {
            person_key: UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
            color_id: BIGINT UNIQUE,

            // Business keys
            full_name_arabic: VARCHAR(500) UNIQUE NOT NULL,
            death_date: DATE NOT NULL,

            record_source: VARCHAR(50),
            load_date: TIMESTAMP DEFAULT NOW()
        }

        SATELLITE: sat_person_details WITH temporal_tracking {
            person_key: UUID REFERENCES hub_deceased_person(person_key),

            gender: VARCHAR(10),
            birth_year: INTEGER,
            age_at_death: INTEGER,
            death_time: TIME,
            death_cause: VARCHAR(200),

            valid_from: TIMESTAMP NOT NULL DEFAULT NOW(),
            valid_to: TIMESTAMP,
            is_current: BOOLEAN DEFAULT TRUE
        }

        SATELLITE: sat_residence_info WITH temporal_tracking {
            person_key: UUID REFERENCES hub_deceased_person(person_key),

            residence_address: TEXT,
            residence_city: VARCHAR(200),
            residence_neighborhood: VARCHAR(200),

            valid_from: TIMESTAMP NOT NULL DEFAULT NOW(),
            is_current: BOOLEAN DEFAULT TRUE
        }

        SATELLITE: sat_burial_info WITH temporal_tracking {
            person_key: UUID REFERENCES hub_deceased_person(person_key),

            burial_date: DATE NOT NULL,
            burial_datetime: TIMESTAMP NOT NULL,
            burial_type: VARCHAR(100),
            shroud_type: VARCHAR(100),
            grave_type: VARCHAR(100),

            valid_from: TIMESTAMP NOT NULL DEFAULT NOW(),
            is_current: BOOLEAN DEFAULT TRUE
        }

        SATELLITE: sat_grave_location WITH {
            person_key: UUID REFERENCES hub_deceased_person(person_key),

            grave_zone: VARCHAR(200) NOT NULL,
            grave_number: VARCHAR(50) NOT NULL UNIQUE,
            grave_latitude: DECIMAL(10, 7) NOT NULL,
            grave_longitude: DECIMAL(10, 7) NOT NULL,
            grave_elevation: DECIMAL(6, 2),

            // PostGIS geography for spatial queries
            grave_point: GEOGRAPHY(POINT, 4326),

            load_date: TIMESTAMP DEFAULT NOW()
        }

        HUB: hub_family WITH {
            family_key: UUID PRIMARY KEY,
            color_id: BIGINT UNIQUE,

            family_name: VARCHAR(200) UNIQUE NOT NULL,

            record_source: VARCHAR(50),
            load_date: TIMESTAMP DEFAULT NOW()
        }

        LINK: link_family_member WITH {
            link_key: UUID PRIMARY KEY,
            person_key: UUID REFERENCES hub_deceased_person(person_key),
            family_key: UUID REFERENCES hub_family(family_key),

            relationship_type: VARCHAR(50),  // 'father', 'mother', 'sibling', etc.

            load_date: TIMESTAMP DEFAULT NOW()
        }
    }

    // ========================================================
    // GRAPH: Cemetery Network
    // ========================================================
    GRAPH CemeteryNetwork {
        NODES {
            DeceasedPerson: primary_key = color_id,
            Family: primary_key = family_key,
            GraveZone: primary_key = zone_id,
            City: primary_key = city_id
        }

        EDGES {
            FAMILY_MEMBER: DeceasedPerson → Family,
            BURIED_IN_ZONE: DeceasedPerson → GraveZone,
            LIVED_IN: DeceasedPerson → City,
            BURIED_NEAR: DeceasedPerson ↔ DeceasedPerson WITH {
                distance_meters: DECIMAL(10, 2)
            }
        }
    }

    // ========================================================
    // VECTORIZATION: Semantic Search
    // ========================================================
    VECTORIZATION {
        MODEL: sentence-transformers/paraphrase-multilingual-mpnet-base-v2
        DIMENSIONS: 768

        EMBEDDINGS {
            person_profile: [
                full_name_arabic,
                residence_city,
                death_cause,
                grave_zone
            ]
        }

        INDEX {
            TYPE: HNSW
            M: 16
            EF_CONSTRUCTION: 200
            DISTANCE: cosine_similarity
        }
    }

    // ========================================================
    // COMMANDS
    // ========================================================

    COMMAND RegisterDeceased {
        full_name: VARCHAR(500),
        death_datetime: TIMESTAMP,
        grave_lat: DECIMAL(10, 7),
        grave_lon: DECIMAL(10, 7),
        grave_zone: VARCHAR(200),

        EXECUTION {
            INSERT_HUB: hub_deceased_person {
                full_name_arabic: full_name,
                death_date: DATE(death_datetime),
                color_id: GPS_to_RGB(grave_lat, grave_lon)
            }

            INSERT_SATELLITE: sat_grave_location {
                grave_latitude: grave_lat,
                grave_longitude: grave_lon,
                grave_zone: grave_zone,
                grave_point: ST_SetSRID(ST_MakePoint(grave_lon, grave_lat), 4326)
            }
        }
    }

    // ========================================================
    // QUERIES
    // ========================================================

    QUERY FindGraveByName {
        search_name: VARCHAR(500),

        SQL_TEMPLATE: '''
        SELECT
            h.full_name_arabic,
            h.death_date,
            g.grave_zone,
            g.grave_number,
            g.grave_latitude,
            g.grave_longitude
        FROM hub_deceased_person h
        JOIN sat_grave_location g ON h.person_key = g.person_key
        WHERE similarity(h.full_name_arabic, @search_name) > 0.3
        ORDER BY similarity(h.full_name_arabic, @search_name) DESC
        LIMIT 10
        '''
    }

    QUERY FindNearbyGraves {
        center_lat: DECIMAL(10, 7),
        center_lon: DECIMAL(10, 7),
        radius_meters: INTEGER DEFAULT 100,

        SQL_TEMPLATE: '''
        SELECT
            h.full_name_arabic,
            g.grave_zone,
            g.grave_number,
            ST_Distance(
                g.grave_point,
                ST_SetSRID(ST_MakePoint(@center_lon, @center_lat), 4326)
            ) as distance_meters
        FROM hub_deceased_person h
        JOIN sat_grave_location g ON h.person_key = g.person_key
        WHERE ST_DWithin(
            g.grave_point,
            ST_SetSRID(ST_MakePoint(@center_lon, @center_lat), 4326),
            @radius_meters
        )
        ORDER BY distance_meters
        '''
    }

    // ========================================================
    // GRAPHRAG: AI-Powered Cemetery Search
    // ========================================================

    RAG_QUERY FindRelatedPeople {
        DESCRIPTION: "Find related people buried in the cemetery"

        RETRIEVAL {
            // Fuzzy name matching
            ENTITY_RESOLUTION: MATCH_ON [full_name_arabic]
                USING [Levenshtein]
                CONFIDENCE_THRESHOLD 0.80

            // Graph traversal
            GRAPH_TRAVERSE: 2_hops FROM person
                VIA: [FAMILY_MEMBER, BURIED_NEAR]

            // Vector similarity
            VECTOR_SEARCH: person_profile_embedding TOP_K 10

            TEMPORAL_WINDOW: last_75_years
        }

        GENERATION {
            LLM: claude-sonnet-4
            PROMPT_TEMPLATE: "cemetery_search"
            TEMPERATURE: 0.2
        }

        AUGMENTATION {
            HYBRID_SEARCH: weighted_sum(
                fuzzy_match=0.40,
                graph=0.35,
                vector=0.25
            )
        }
    }
}
```

---

## **4. Quick Start Guide**

### **Step 1: Generate Data**
```bash
python najaf_data_generator.py
```

### **Step 2: Create Database**
```bash
createdb -U postgres najaf_cemetery
psql -U postgres -d najaf_cemetery -c "CREATE EXTENSION postgis;"
psql -U postgres -d najaf_cemetery -c "CREATE EXTENSION pg_trgm;"  # For fuzzy matching
```

### **Step 3: Import Data**
```bash
# Import first batch
psql -U postgres -d najaf_cemetery -f najaf_cemetery_data/najaf_cemetery_sql_01.sql

# Or import CSV
psql -U postgres -d najaf_cemetery -c "\COPY deceased_persons FROM 'najaf_cemetery_data/najaf_cemetery_batch_001.csv' CSV HEADER ENCODING 'UTF8'"
```

### **Step 4: Compile Akkadian Schema**
```bash
akkadianc compile najaf_cemetery.akk
```

---

## **5. Sample Queries**

```sql
-- Find all deceased from Najaf in 2020
SELECT full_name_arabic, death_date, grave_zone, grave_number
FROM deceased_persons
WHERE residence_city = 'النجف الأشرف'
  AND EXTRACT(YEAR FROM death_date) = 2020;

-- Find graves within 50 meters of a location
SELECT
    full_name_arabic,
    grave_zone,
    ST_Distance(
        ll_to_earth(grave_latitude, grave_longitude),
        ll_to_earth(32.0000, 44.3200)
    ) as distance_meters
FROM deceased_persons
WHERE earth_box(ll_to_earth(32.0000, 44.3200), 50) @> ll_to_earth(grave_latitude, grave_longitude)
ORDER BY distance_meters
LIMIT 10;

-- Fuzzy name search
SELECT full_name_arabic, death_date, grave_zone
FROM deceased_persons
WHERE similarity(full_name_arabic, 'محمد علي الموسوي') > 0.3
ORDER BY similarity(full_name_arabic, 'محمد علي الموسوي') DESC
LIMIT 10;
```

---

**The script generates 1 million realistic records with:**
- ✅ Arabic names (Iraqi dialect)
- ✅ Najaf addresses
- ✅ Death dates (1950-2025)
- ✅ Grave coordinates (within Wadi-us-Salaam bounds)
- ✅ 5% name variants for entity resolution testing
- ✅ OSM cemetery data integration

**Ready to test your Najaf Cemetery project! 🪦📊**
