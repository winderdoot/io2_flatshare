import { useState } from "react";
import styles from "./SearchFilters.module.css";
import { locationConfig } from "./locationConfig";

type Profile = "student" | "worker" | "family" | "";

interface Filters {
  city: City | "";
  district: string;
  minPrice: string;
  maxPrice: string;
  petsAllowed: boolean;
  nonSmokingOnly: boolean;
  closeToShops: string;
  profile: Profile;
  minArea: string;
  maxArea: string;
  startDate: string;
}

const profiles: Profile[] = ["student", "worker", "family"];

type City = keyof typeof locationConfig;

export default function SearchFilters() {
  const [filters, setFilters] = useState<Filters>({
    city: "",
    district: "",
    minPrice: "",
    maxPrice: "",
    petsAllowed: false,
    nonSmokingOnly: false,
    closeToShops: "",
    profile: "",
    minArea: "",
    maxArea: "",
    startDate: ""
  });

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;

    setFilters((prev) => ({
      ...prev,
      [name]:
        type === "checkbox"
          ? (e.target as HTMLInputElement).checked
          : value
    }));
  };

  const handleCityChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const city = e.target.value as City | "";

    setFilters((prev) => ({
      ...prev,
      city,
      district: ""
    }));
  };

  const handleSubmit = () => {
    console.log(filters);
  };

  return (
    <div className={styles.container}>
      <div className={styles.row}>
        <select
          name="city"
          value={filters.city}
          onChange={handleCityChange}
          className={styles.select}
        >
          <option value="">Miasto</option>
          {Object.keys(locationConfig).map((city) => (
            <option key={city} value={city}>
              {city}
            </option>
          ))}
        </select>

        <select
          name="district"
          value={filters.district}
          onChange={handleChange}
          disabled={!filters.city}
          className={styles.select}
        >
          <option value="">Dzielnica</option>
          {filters.city &&
            locationConfig[filters.city].map((d) => (
              <option key={d} value={d}>
                {d}
              </option>
            ))}
        </select>

        <select
          name="profile"
          value={filters.profile}
          onChange={handleChange}
          className={styles.select}
        >
          <option value="">Profil</option>
          {profiles.map((p) => (
            <option key={p} value={p}>
              {p}
            </option>
          ))}
        </select>
      </div>

      <div className={styles.row}>
        <input
          type="number"
          name="minPrice"
          placeholder="Cena od"
          value={filters.minPrice}
          onChange={handleChange}
          className={styles.input}
        />
        <input
          type="number"
          name="maxPrice"
          placeholder="Cena do"
          value={filters.maxPrice}
          onChange={handleChange}
          className={styles.input}
        />

        <input
          type="number"
          name="minArea"
          placeholder="m² od"
          value={filters.minArea}
          onChange={handleChange}
          className={styles.input}
        />
        <input
          type="number"
          name="maxArea"
          placeholder="m² do"
          value={filters.maxArea}
          onChange={handleChange}
          className={styles.input}
        />
      </div>

      <div className={styles.row}>
        <label className={styles.checkboxGroup}>
          <input
            type="checkbox"
            name="petsAllowed"
            checked={filters.petsAllowed}
            onChange={handleChange}
          />
          Zwierzęta
        </label>

        <label className={styles.checkboxGroup}>
          <input
            type="checkbox"
            name="nonSmokingOnly"
            checked={filters.nonSmokingOnly}
            onChange={handleChange}
          />
          Niepalący
        </label>

        <input
          type="number"
          name="closeToShops"
          placeholder="Sklepy (liczba)"
          value={filters.closeToShops}
          onChange={handleChange}
          className={styles.input}
        />

        <input
          type="date"
          name="startDate"
          value={filters.startDate}
          onChange={handleChange}
          className={styles.input}
        />
      </div>

      <button className={styles.button} onClick={handleSubmit}>
        Wyszukaj
      </button>
    </div>
  );
}