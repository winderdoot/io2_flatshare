import { useState } from "react";
import styles from "./SearchFilters.module.css";
import { locationConfig } from "./locationConfig";
import FormattedNumberInput from "../FormattedNumberInput/FormattedNumberInput";


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
              {city.charAt(0).toUpperCase() + city.slice(1)}
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
        <div className={styles.col}>
          <label>Cena</label>
          <div className={styles.row}>          
            <FormattedNumberInput
              value={filters.minPrice}
              onChange={(val) =>
                setFilters((prev) => ({ ...prev, minPrice: val }))
              }
              placeholder="Od"
              suffix="zł"
            />
            <FormattedNumberInput
              value={filters.maxPrice}
              onChange={(val) =>
                setFilters((prev) => ({ ...prev, maxPrice: val }))
              }
              placeholder="Do"
              suffix="zł"
            />
          </div>
        </div>

        <div className={styles.col}>
          <label>Powierzchnia</label>
          <div className={styles.row}>
            <FormattedNumberInput
              value={filters.minArea}
              onChange={(val) =>
                setFilters((prev) => ({ ...prev, minArea: val }))
              }
              placeholder="Od"
              suffix="m²"
            />
            <FormattedNumberInput
              value={filters.maxArea}
              onChange={(val) =>
                setFilters((prev) => ({ ...prev, maxArea: val }))
              }
              placeholder="Do"
              suffix="m²"
            />
          </div>
        </div>
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

        <div className={styles.col}>
          <label>Liczba sklepów</label>
          <FormattedNumberInput
            value={filters.closeToShops}
            onChange={(val) =>
              setFilters((prev) => ({ ...prev, closeToShops: val }))
            }
            placeholder="Od"
            suffix="sklepów"
          >
          </FormattedNumberInput>
        </div>

        <div className={styles.col}>
          <label>Od kiedy</label>
          <input
            type="date"
            name="startDate"
            value={filters.startDate}
            onChange={handleChange}
            className={styles.input}
          />
        </div>
      </div>

      <button className={styles.button} onClick={handleSubmit}>
        Wyszukaj
      </button>
    </div>
  );
}