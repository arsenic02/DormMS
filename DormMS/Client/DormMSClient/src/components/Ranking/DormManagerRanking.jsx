import { useState, useEffect } from 'react';
import {
  rankFreshmen,
  rankOlderStudents,
  getSavedRankingResults,
  assignOlderStudentsToDorms,
  assignFreshmenToDorms,
  assignSingleStudent
} from '../../api/ranking';
import styles from './DormManagerRanking.module.css';

const AVERAGE_SALARY = 121650;

function getSocialBonus(incomePerMember) {
  return incomePerMember > 0 && incomePerMember < AVERAGE_SALARY * 0.5 ? 1 : 0;
}

function getEspbBonus(espb) {
  if (espb > 240) return 2;
  if (espb >= 120) return 1;
  return 0;
}

function formatNumber(value) {
  return Number(value).toLocaleString('sr-RS', { maximumFractionDigits: 2 });
}

// Backend enum: Assigned=0, Waitlisted=1, NotAssigned=2, Error=3
const ASSIGNMENT_STATUS = {
  ASSIGNED: 0,
  WAITLISTED: 1,
  NOT_ASSIGNED: 2,
  ERROR: 3,
  ASSIGNED_NO_ROOM: 4,
};

function StatusBadge({ status }) {
  if (status === ASSIGNMENT_STATUS.ASSIGNED) {
    return <span className={styles.badgeAssigned}>Primljen</span>;
  }
  if (status === ASSIGNMENT_STATUS.ASSIGNED_NO_ROOM) {
    return <span className={styles.badgeNoRoom}>Primljen — čeka sobu</span>;
  }
  if (status === ASSIGNMENT_STATUS.WAITLISTED) {
    return <span className={styles.badgeWaitlisted}>Lista čekanja</span>;
  }
  if (status === ASSIGNMENT_STATUS.NOT_ASSIGNED) {
    return <span className={styles.badgeNeutral}>Nije rangiran</span>;
  }
  if (status === ASSIGNMENT_STATUS.ERROR) {
    return <span className={styles.badgeNeutral}>Greška</span>;
  }
  return <span className={styles.badgeNeutral}>{status}</span>;
}

function OlderStudentsTable({ students, onAssign, assigningId, assignError, roomsAssigned }) {

  const firstUnassignedIdx = students.findIndex(
    (s) =>
      (s.status === ASSIGNMENT_STATUS.ASSIGNED && !s.isPhysicallyAssigned) ||
      s.status === ASSIGNMENT_STATUS.ASSIGNED_NO_ROOM
  );
  //const firstUnassignedIdx = students.findIndex((s) => s.status !== ASSIGNMENT_STATUS.ASSIGNED);

  return (
    <table className={styles.table}>
      <thead>
        <tr>
          <th>Ime i prezime</th>
          <th>Broj indeksa</th>
          <th>Prosek (fakultet)</th>
          <th>ESPB</th>
          <th>Godina studija</th>
          <th>Socijalni bonus</th>
          <th>Dodatak</th>
          <th>Ukupno</th>
          <th className={styles.statusCell}>Status</th>
        </tr>
      </thead>
      <tbody>
        {students.map((s, idx) => (
          <tr key={s.studentId}
            className={
              s.status === ASSIGNMENT_STATUS.ASSIGNED
                ? styles.rowAssigned
                : s.status === ASSIGNMENT_STATUS.ASSIGNED_NO_ROOM
                  ? styles.rowNoRoom
                  : undefined
            }>
            <td>{s.studentName} {s.studentSurname}</td>
            <td className={styles.muted}>{s.studentIDNumber}</td>
            <td className={styles.numeric}>{formatNumber(s.averageGrade)}</td>
            <td className={styles.numeric}>{s.espb}</td>
            <td className={styles.numeric}>{s.yearsOfStudy}</td>
            <td className={styles.numeric}>{getSocialBonus(s.incomePerMember)}</td>
            <td className={styles.numeric}>{getEspbBonus(s.espb)}</td>
            <td className={styles.numericBold}>{formatNumber(s.calculatedPoints)}</td>
            <td className={styles.statusCell}>
              <StatusBadge status={s.status} />
              {roomsAssigned && idx === firstUnassignedIdx && (
                <div className={styles.assignBox}>
                  <button
                    className={styles.assignSingleButton}
                    onClick={() => onAssign(s.rankingResultId)}
                    disabled={assigningId === s.rankingResultId}
                    type="button"
                  >
                    <i className="ti ti-door-enter" aria-hidden="true" />
                    {assigningId === s.rankingResultId ? 'Ubacujem...' : 'Ubaci u dom'}
                  </button>
                  {/* {assignError?.rankingResultId === s.rankingResultId && (
                    <span className={styles.assignErrorText}>{assignError.message}</span>
                  )} */}
                </div>
              )}
            </td>
            {/* <td className={styles.statusCell}>
              <StatusBadge status={s.status} />
              {idx === firstUnassignedIdx && (
                <button
                  className={styles.assignSingleButton}
                  onClick={() => onAssign(s.rankingResultId)}
                  disabled={assigningId === s.rankingResultId}
                  type="button"
                >
                   <i className="ti ti-door-enter" aria-hidden="true" />
                  {assigningId === s.rankingResultId ? 'Ubacujem...' : 'Ubaci u dom'}
                </button>

              )}
            </td> */}
          </tr>
        ))}
      </tbody>
    </table>
  );
}
// function OlderStudentsTable({ students }) {
//   return (
//     <table className={styles.table}>
//       <thead>
//         <tr>
//           <th>Ime i prezime</th>
//           <th>Broj indeksa</th>
//           <th>Prosek (fakultet)</th>
//           <th>ESPB</th>
//           <th>Godina studija</th>
//           <th>Socijalni bonus</th>
//           <th>Dodatak</th>
//           <th>Ukupno</th>
//           <th className={styles.statusCell}>Status</th>
//         </tr>
//       </thead>
//       <tbody>
//         {students.map((s) => (
//           <tr key={s.studentId}
//             // className={s.status === ASSIGNMENT_STATUS.ASSIGNED ? styles.rowAssigned : undefined}
//             className={
//               s.status === ASSIGNMENT_STATUS.ASSIGNED
//                 ? styles.rowAssigned
//                 : s.status === ASSIGNMENT_STATUS.ASSIGNED_NO_ROOM
//                   ? styles.rowNoRoom
//                   : undefined
//             }>
//             <td>{s.studentName} {s.studentSurname}</td>
//             <td className={styles.muted}>{s.studentIDNumber}</td>
//             <td className={styles.numeric}>{formatNumber(s.averageGrade)}</td>
//             <td className={styles.numeric}>{s.espb}</td>
//             <td className={styles.numeric}>{s.yearsOfStudy}</td>
//             <td className={styles.numeric}>{getSocialBonus(s.incomePerMember)}</td>
//             <td className={styles.numeric}>{getEspbBonus(s.espb)}</td>
//             <td className={styles.numericBold}>{formatNumber(s.calculatedPoints)}</td>
//             <td className={styles.statusCell}><StatusBadge status={s.status} /></td>
//           </tr>
//         ))}
//       </tbody>
//     </table>
//   );
// }

function FreshmenTable({ students, onAssign, assigningId, assignError, roomsAssigned }) {
  const firstUnassignedIdx = students.findIndex(
    (s) =>
      (s.status === ASSIGNMENT_STATUS.ASSIGNED && !s.isPhysicallyAssigned) ||
      s.status === ASSIGNMENT_STATUS.ASSIGNED_NO_ROOM
  );
  //const firstUnassignedIdx = students.findIndex((s) => s.status !== ASSIGNMENT_STATUS.ASSIGNED);
  return (
    <table className={styles.table}>
      <thead>
        <tr>
          <th>Ime i prezime</th>
          <th>Broj indeksa</th>
          <th>Prosek (srednja škola)</th>
          <th>Socijalni bonus</th>
          <th>Ukupno</th>
          <th className={styles.statusCell}>Status</th>
        </tr>
      </thead>
      <tbody>
        {students.map((s, idx) => (
          <tr key={s.studentId}
            // className={s.status === ASSIGNMENT_STATUS.ASSIGNED ? styles.rowAssigned : undefined}
            className={
              s.status === ASSIGNMENT_STATUS.ASSIGNED
                ? styles.rowAssigned
                : s.status === ASSIGNMENT_STATUS.ASSIGNED_NO_ROOM
                  ? styles.rowNoRoom
                  : undefined
            }>
            <td>{s.studentName} {s.studentSurname}</td>
            <td className={styles.muted}>{s.studentIDNumber}</td>
            <td className={styles.numeric}>{formatNumber(s.highSchoolAverageGrade)}</td>
            <td className={styles.numeric}>{getSocialBonus(s.incomePerMember)}</td>
            <td className={styles.numericBold}>{formatNumber(s.calculatedPoints)}</td>

            <td className={styles.statusCell}>
              <StatusBadge status={s.status} />
              {roomsAssigned && idx === firstUnassignedIdx && (
                <div className={styles.assignBox}>
                  <button
                    className={styles.assignSingleButton}
                    onClick={() => onAssign(s.rankingResultId)}
                    disabled={assigningId === s.rankingResultId}
                    type="button"
                  >
                    <i className="ti ti-door-enter" aria-hidden="true" />
                    {assigningId === s.rankingResultId ? 'Ubacujem...' : 'Ubaci u dom'}
                  </button>
                  {/* {assignError?.rankingResultId === s.rankingResultId && (
                    <span className={styles.assignErrorText}>{assignError.message}</span>
                  )} */}
                </div>
              )}
            </td>
            {/* <td className={styles.statusCell}>
              <StatusBadge status={s.status} />
              {idx === firstUnassignedIdx && (
                <button
                  className={styles.assignSingleButton}
                  onClick={() => onAssign(s.rankingResultId)}
                  disabled={assigningId === s.rankingResultId}
                  type="button"
                >
                  <i className="ti ti-door-enter" aria-hidden="true" />
                  {assigningId === s.rankingResultId ? 'Ubacujem...' : 'Ubaci u dom'}
                </button>
              )}
            </td> */}
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function FacultyAccordion({ facultyName, students, isFreshmanView, isOpen, onToggle, onAssign, assigningId, assignError, roomsAssigned }) {
  const assignedCount = students.filter((s) => s.status === ASSIGNMENT_STATUS.ASSIGNED).length;
  const noRoomCount = students.filter((s) => s.status === ASSIGNMENT_STATUS.ASSIGNED_NO_ROOM).length;

  return (
    <div className={styles.card}>
      <button className={styles.cardHeader} onClick={onToggle} type="button">
        <div className={styles.cardHeaderLeft}>
          <i className={`ti ti-chevron-right ${styles.chevron} ${isOpen ? styles.chevronOpen : ''}`} aria-hidden="true" />
          <span className={styles.facultyName}>{facultyName}</span>
        </div>
        <span className={styles.summary}>
          {students.length} prijavljenih &middot; {assignedCount} primljeno
          {noRoomCount > 0 && (
            <>
              {' '}&middot;{' '}
              <span className={styles.summaryNoRoom}>{noRoomCount} čeka sobu</span>
            </>
          )}
        </span>
      </button>

      {isOpen && (
        <div className={styles.cardBody}>
          {isFreshmanView ? (
            <FreshmenTable students={students} onAssign={onAssign} assigningId={assigningId} assignError={assignError} roomsAssigned={roomsAssigned} />
          ) : (
            <OlderStudentsTable students={students} onAssign={onAssign} assigningId={assigningId} assignError={assignError} roomsAssigned={roomsAssigned} />
          )}
        </div>
      )}
    </div>
  );
}

export default function DormManagerRanking({ competitionId, isFreshmanView, isCompleted }) {
  const [rankingData, setRankingData] = useState(null);
  const [openFaculties, setOpenFaculties] = useState({});
  const [loading, setLoading] = useState(false);
  const [loadingSaved, setLoadingSaved] = useState(false);
  const [error, setError] = useState(null);

  const [assignmentResult, setAssignmentResult] = useState(null);
  const [assigningRooms, setAssigningRooms] = useState(false);
  const [assignmentError, setAssignmentError] = useState(null);

  const [roomsAssignedLocally, setRoomsAssignedLocally] = useState(false);
  const roomsAssigned = roomsAssignedLocally || !!rankingData?.roomsAssigned;

  const [justRanked, setJustRanked] = useState(false);
  const effectivelyCompleted = isCompleted || justRanked;

  const [assigningStudentId, setAssigningStudentId] = useState(null);
  const [singleAssignError, setSingleAssignError] = useState(null);

  // Ako je konkurs vec zavrsen (Completed), automatski ucitaj sacuvane rezultate
  // umesto da korisnik mora da klikne "Pokreni rangiranje" (sto vise ne bi smelo
  // da se radi ponovo).
  useEffect(() => {
    if (!isCompleted) return;

    async function fetchSavedResults() {
      setLoadingSaved(true);
      setError(null);
      try {
        const result = await getSavedRankingResults(competitionId, isFreshmanView);
        setRankingData(result);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoadingSaved(false);
      }
    }

    fetchSavedResults();
  }, [isCompleted, competitionId, isFreshmanView]);

  async function handleRunRanking() {
    setLoading(true);
    setError(null);

    try {
      const result = isFreshmanView
        ? await rankFreshmen(competitionId)
        : await rankOlderStudents(competitionId);

      setRankingData(result);
      setJustRanked(true);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  async function handleAssignRooms() {
    setAssigningRooms(true);
    setAssignmentError(null);

    try {
      const result = isFreshmanView
        ? await assignFreshmenToDorms(competitionId)
        : await assignOlderStudentsToDorms(competitionId);

      setAssignmentResult(result);
      setRoomsAssignedLocally(true);

      const refreshed = await getSavedRankingResults(competitionId, isFreshmanView);
      setRankingData(refreshed);
    } catch (err) {
      setAssignmentError(err.message);
    } finally {
      setAssigningRooms(false);
    }
  }

  async function handleAssignSingle(rankingResultId) {
    setAssigningStudentId(rankingResultId);
    setSingleAssignError(null);
    try {
      await assignSingleStudent(rankingResultId);
      const refreshed = await getSavedRankingResults(competitionId, isFreshmanView);
      setRankingData(refreshed);
    } catch (err) {
      setSingleAssignError({ rankingResultId, message: err.message }); //ubaci gdde treba to
      setTimeout(() => setSingleAssignError(null), 3000);
    } finally {
      setAssigningStudentId(null);
    }
  }

  function toggleFaculty(facultyName) {
    setOpenFaculties((prev) => ({ ...prev, [facultyName]: !prev[facultyName] }));
  }

  const faculties = rankingData ? Object.entries(rankingData.rankingsByFaculty) : [];
  const allStudents = rankingData
    ? Object.values(rankingData.rankingsByFaculty).flat()
    : [];
  const noRoomCount = allStudents.filter((s) => s.status === ASSIGNMENT_STATUS.ASSIGNED_NO_ROOM).length;

  return (
    <div className={styles.container}>
      <div className={styles.toolbar}>
        <h2 className={styles.title}>
          {isFreshmanView ? 'Rangiranje brucoša' : 'Rangiranje starijih studenata'}
        </h2>
        <button
          className={styles.runButton}
          onClick={handleRunRanking}
          disabled={loading || effectivelyCompleted}
          type="button"
          title={effectivelyCompleted ? 'Konkurs je već rangiran i ne može se rangirati ponovo.' : undefined}
        >
          {loading ? 'Rangiranje u toku...' : effectivelyCompleted ? 'Konkurs je već rangiran' : 'Pokreni rangiranje'}
        </button>
      </div>

      {loadingSaved && <p className={styles.totalInfo}>Učitavanje sačuvanih rezultata...</p>}
      {error && <p className={styles.error}>{error}</p>}

      {rankingData && (
        <p className={styles.totalInfo}>
          Ukupno studenata: {rankingData.totalStudents}
        </p>
      )}

      {effectivelyCompleted && (
        <div className={styles.assignBar}>
          <button
            className={styles.assignButton}
            onClick={handleAssignRooms}
            disabled={assigningRooms || roomsAssigned}
            type="button"
          >
            {assigningRooms
              ? 'Raspodela u toku...'
              : roomsAssigned
                ? 'Sobe su raspodeljene'
                : 'Raspodeli sobe'}
          </button>

          {assignmentError && <p className={styles.error}>{assignmentError}</p>}

          {assignmentResult && (
            <p className={styles.totalInfo}>
              Dodeljeno: {assignmentResult.successfullyAssigned} &middot; Na čekanju za sobu: {assignmentResult.waitlisted}
            </p>
          )}
        </div>
      )}

      {/* {singleAssignError && <p className={styles.error}>{singleAssignError}</p>} */}

      {faculties
        .sort(([, a], [, b]) => b.length - a.length)
        .map(([facultyName, students]) => (
          <FacultyAccordion
            key={facultyName}
            facultyName={facultyName}
            students={students}
            isFreshmanView={isFreshmanView}
            isOpen={!!openFaculties[facultyName]}
            onToggle={() => toggleFaculty(facultyName)}
            onAssign={handleAssignSingle}
            assigningId={assigningStudentId}
            assignError={singleAssignError}
            roomsAssigned={roomsAssigned}
          />
        ))}
      {singleAssignError && (
        <div className={styles.toast}>
          {singleAssignError.message}
        </div>
      )}
    </div>

  );
}